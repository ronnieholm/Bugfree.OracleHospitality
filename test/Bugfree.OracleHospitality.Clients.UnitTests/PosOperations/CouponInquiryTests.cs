using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.Seedwork;
using Bugfree.OracleHospitality.Clients.PosOperations;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies;
using Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;
using XE = System.Xml.Linq.XElement;
using Action = Bugfree.OracleHospitality.Clients.PosParselets.Action;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosOperations;

public class CouponInquiryTests
{
    private readonly IOptions<OracleHospitalityClientsOptions> _options;
    private readonly IPosMessageSequencingStrategy _messageSequencingStrategy;
    private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

    public CouponInquiryTests()
    {
        _options = Options.Create(
            A.OracleHospitalityClientOptions
                .WithTerminalIdLowerBound(0)
                .WithTerminalIdUpperBound(100).Build());
        _messageSequencingStrategy = FixedStrategy.Default();
        _executorLogger = new Mock<ILogger<OracleHospitalityExecutor>>().Object;
    }

    [Fact]
    public void request_generation()
    {
        using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
        {
            var request = new CouponInquiryRequest(
                TimeProvider.Now,
                new MessageId(new TerminalId(0), new SequenceNumber(42), new CheckNumber(13)),
                new AccountNumber("123"));
            var requestXml = request.BuildRequestDocument();

            const string expected = @"
                <SVCMessage version=""1"" 
                            posIntfcName=""posInterfaceName"" 
                            posIntfcVersion=""1.00"" 
                            language=""en-US"" 
                            currency=""DKK"" 
                            sequence=""42"" 
                            retransmit=""n"">
                    <RequestCode>COUPON_INQUIRY</RequestCode>
                    <TraceID>190826145135N420013</TraceID>
                    <TerminalID>0</TerminalID>
                    <TerminalType>Service</TerminalType>
                    <LocalDate>20190826</LocalDate>
                    <LocalTime>145135</LocalTime>                        
                    <Amount>0.00</Amount>
                    <LocalCurrency>DKK</LocalCurrency>
                    <BusinessDate>20190826</BusinessDate>
                    <TransactionEmployee>0</TransactionEmployee>
                    <RevenueCenter>0</RevenueCenter>
                    <CheckNumber>0013</CheckNumber>
                    <SVAN>123</SVAN>
                </SVCMessage>";

            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }
    }

    [Fact]
    public async Task invalid_account()
    {
        const string response = @"
            <?xml version=""1.0""?>
            <SVCMessage language=""en-US"" 
                        retransmit=""n"" 
                        sequence=""00"" 
                        currency=""DKK"" 
                        posIntfcVersion=""1.00"" 
                        posIntfcName=""posInterfaceName"" 
                        version=""1"" 
                        hostVersion=""9.1.0000.2301"">
                <RequestCode>COUPON_INQUIRY</RequestCode>
                <TraceID>190820113916N000000</TraceID>
                <Amount>0.00</Amount>
                <SVAN>123</SVAN>
                <ResponseCode hostCode=""1"">D</ResponseCode>
                <DisplayMessage>There is no account number or the account number (123) is too short</DisplayMessage>
            </SVCMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new PosClient(_messageSequencingStrategy, executor);

        using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-20T11:39:16")))
        {
            var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(
                () => sut.CouponInquiryAsync(new AccountNumber("123")));
            Assert.Equal("1", e.Code);
            Assert.Equal("There is no account number or the account number (123) is too short", e.Message);
        }
    }

    [Fact]
    public async Task valid_account_with_no_coupons()
    {
        const string response = @"
            <?xml version=""1.0"" encoding=""UTF-8""?>
            <SVCMessage hostVersion=""9.1.0000.2301"" 
                        version=""1"" 
                        posIntfcName=""posInterfaceName"" 
                        posIntfcVersion=""1.00"" 
                        language=""en-US"" 
                        currency=""DKK"" 
                        sequence=""0""
                        retransmit=""n"">
                <RequestCode>COUPON_INQUIRY</RequestCode>
                <TraceID>190826145135N000000</TraceID>
                <Amount>0.00</Amount>
                <SVAN>42</SVAN>
                <AccountCurrency>DKK</AccountCurrency>
                <ExchangeRate>1.00</ExchangeRate>
                <ResponseCode>A</ResponseCode>
                <DisplayMessage>There are no eligible coupons for this card.</DisplayMessage>
            </SVCMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new PosClient(_messageSequencingStrategy, executor);

        using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
        {
            var actual = await sut.CouponInquiryAsync(new AccountNumber("42"));
            Assert.Equal("There are no eligible coupons for this card.", actual.DisplayMessage.Value);
            Assert.Empty(actual.Actions);
        }
    }
        
    [Fact]
    public async Task valid_account_with_one_coupon()
    {
        var response = @"
            <SVCMessage hostVersion=""9.1.0000.2301"" 
                        version=""1"" 
                        posIntfcName=""posInterfaceName"" 
                        posIntfcVersion=""1.00"" 
                        language=""en-US"" 
                        currency=""DKK"" 
                        sequence=""00"" 
                        retransmit=""n"">
                <RequestCode>COUPON_INQUIRY</RequestCode>
                <TraceID>190826145135N000000</TraceID>
                <Amount>0.00</Amount>
                <SVAN>123</SVAN>
                <AccountCurrency>DKK</AccountCurrency>
                <ExchangeRate>1.00</ExchangeRate>
                <ResponseCode>A</ResponseCode>
                <Actions>
                    <Action>
                        <Type tid=""12"">Accept Coupon</Type>
                        <Data pid=""9"">1004019</Data>
                        <Code>10DKK</Code>
                        <Text>Coupon: 10 DKK, Always Valid</Text>
                    </Action>
                </Actions>
                <PrintLines />
                <DisplayMessage>There is an eligible coupon for this card.</DisplayMessage>
            </SVCMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new PosClient(_messageSequencingStrategy, executor);

        using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
        {
            var actual = await sut.CouponInquiryAsync(new AccountNumber("123"));

            // Attributes
            Assert.Equal("9.1.0000.2301", actual.HostVersion.Value);
            Assert.Equal("1", actual.Version.Value);
            Assert.Equal("posInterfaceName", actual.PosInterfaceName.Value);
            Assert.Equal("en-US", actual.Language.Value);
            Assert.Equal(Currency.Kind.DKK, actual.Currency.Value);
            Assert.Equal(0, actual.SequenceNumber.Value);
            Assert.Equal(Transmission.Kind.Normal, actual.Transmission.Value);

            // Elements
            Assert.Equal(TransactionKind.COUPON_INQUIRY, actual.RequestCode.Value);
            Assert.Equal("190826145135N000000", actual.TraceId.ToString());
            Assert.Equal(0, actual.Amount.Value);
            Assert.Equal("123", actual.AccountNumber.Value);
            Assert.Equal(Currency.Kind.DKK, actual.AccountCurrency.Value);
            Assert.Equal(1.00m, actual.ExchangeRate.Value);
            Assert.Equal(ResponseCode.Kind.Approved, actual.ResponseCode.Value);
            Assert.Single(actual.Actions);
            var action = actual.Actions[0];
            AssertAction(
                action,
                ActionType.TransactionId.AcceptCoupon,
                ActionData.PromptId.PleaseEnterCoupon,
                "1004019",
                "10DKK",
                "Coupon: 10 DKK, Always Valid");
            Assert.Empty(actual.PrintLines.Values);
            Assert.Equal("There is an eligible coupon for this card.", actual.DisplayMessage.Value);
        }
    }

    [Fact]
    public async Task valid_account_with_two_coupons()
    {
        const string response = @"
            <SVCMessage hostVersion=""9.1.0000.2301"" 
                        version=""1"" 
                        posIntfcName=""posInterfaceName"" 
                        posIntfcVersion=""1.00"" 
                        language=""en-US"" 
                        currency=""DKK"" 
                        sequence=""00"" 
                        retransmit=""n"">
                <RequestCode>COUPON_INQUIRY</RequestCode>
                <TraceID>190826145135N000000</TraceID>
                <Amount>0.00</Amount>
                <SVAN>123</SVAN>
                <AccountCurrency>DKK</AccountCurrency>
                <ExchangeRate>1.00</ExchangeRate>
                <ResponseCode>A</ResponseCode>
                <Actions>
                <Action>
                    <Type tid=""12"">Accept Coupon</Type>
                    <Data pid=""9"">1005016</Data>
                    <Code>10DKK</Code>
                    <Text>Coupon: 10 DKK, Always Valid</Text>
                </Action>
                <Action>
                    <Type tid=""12"">Accept Coupon</Type>
                    <Data pid=""9"">1006014</Data>
                    <Code>10DKK</Code>
                    <Text>Coupon: 10 DKK, Always Valid</Text>
                </Action>
                </Actions>
                <PrintLines/>
                <DisplayMessage>There are 2 eligible coupons for this card.</DisplayMessage>
            </SVCMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new PosClient(_messageSequencingStrategy, executor);

        using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
        {
            var actual = await sut.CouponInquiryAsync(new AccountNumber("123"));
            Assert.Equal("There are 2 eligible coupons for this card.", actual.DisplayMessage.Value);

            Assert.Equal(2, actual.Actions.Count);
            var a = actual.Actions[0];
            AssertAction(
                a,
                ActionType.TransactionId.AcceptCoupon,
                ActionData.PromptId.PleaseEnterCoupon,
                "1005016",
                "10DKK",
                "Coupon: 10 DKK, Always Valid");
            var b = actual.Actions[1];
            AssertAction(
                b,
                ActionType.TransactionId.AcceptCoupon,
                ActionData.PromptId.PleaseEnterCoupon,
                "1006014",
                "10DKK",
                "Coupon: 10 DKK, Always Valid");
        }
    }

    private void AssertAction(Action a, ActionType.TransactionId tid, ActionData.PromptId pid, string couponSequenceNumber, string code, string text)
    {
        Assert.Equal(tid, a.Type.Id);
        Assert.Equal(pid, a.Data.Id);
        Assert.Equal(couponSequenceNumber, a.Data.Value.Value);
        Assert.Equal(code, a.Code.Value);
        Assert.Equal(text, a.Text.Value.Value);
    }
}