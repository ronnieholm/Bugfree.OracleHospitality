using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.Seedwork;
using Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;
using Bugfree.OracleHospitality.Clients.PosOperations;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosOperations;

public class CouponIssueTests
{
    private readonly IOptions<OracleHospitalityClientsOptions> _options;
    private readonly IPosMessageSequencingStrategy _messageSequencingStrategy;
    private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

    public CouponIssueTests()
    {
        _options =
            Options.Create(
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
            var request = new CouponIssueRequest(
                TimeProvider.Now,
                new MessageId(new TerminalId(0), new SequenceNumber(42), new CheckNumber(13)),
                new AccountNumber("123"),
                new CouponCode("10DKK"));
            var requestXml = request.BuildRequestDocument();

            const string expected = @"
                <SVCMessage version=""1"" 
                            posIntfcName=""posInterfaceName"" 
                            posIntfcVersion=""1.00"" 
                            language=""en-US"" 
                            currency=""DKK"" 
                            sequence=""42"" 
                            retransmit=""n"">
                    <RequestCode>SV_ISSUE_COUPON</RequestCode>
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
                    <CouponCode>10DKK</CouponCode>
                </SVCMessage>";

            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }
    }

    [Fact]
    public async Task valid_coupon_code()
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
                <RequestCode>SV_ISSUE_COUPON</RequestCode>
                <TraceID>190826145135N000000</TraceID>
                <Amount>0.00</Amount>
                <SVAN>123</SVAN>
                <AccountCurrency>DKK</AccountCurrency>
                <ExchangeRate>1.00</ExchangeRate>
                <ItemType>T</ItemType>
                <ItemNumber>1</ItemNumber>
                <PrintLines />
                <CouponCode>1006022</CouponCode>
                <ResponseCode>A</ResponseCode>
                <DisplayMessage>Coupon (10 DKK) has been issued to this account.</DisplayMessage>
            </SVCMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new PosClient(_messageSequencingStrategy, executor);

        using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
        {
            var actual = await sut.CouponIssueAsync(new AccountNumber("123"), new CouponCode("doesNotMatter"));
            Assert.Equal("1006022", actual.CouponCode.Value);
            Assert.Equal("Coupon (10 DKK) has been issued to this account.", actual.DisplayMessage.Value);
            Assert.Equal(ItemType.Kind.Tender, actual.ItemType.Value);
            Assert.Equal(1, actual.ItemNumber.Value);
            Assert.Empty(actual.PrintLines.Values);
            Assert.Equal(Currency.Kind.DKK, actual.AccountCurrency.Value);
            Assert.Equal(1.00m, actual.ExchangeRate.Value);
        }
    }

    [Fact]
    public async Task invalid_coupon_code()
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
                <RequestCode>SV_ISSUE_COUPON</RequestCode>
                <TraceID>190827125052N000000</TraceID>
                <Amount>0.00</Amount>
                <SVAN>123</SVAN>
                <AccountCurrency>DKK</AccountCurrency>
                <ExchangeRate>1.00</ExchangeRate>
                <ItemType>T</ItemType>
                <ItemNumber>1</ItemNumber>
                <ResponseCode hostCode=""127"">D</ResponseCode>
                <DisplayMessage>Coupon code (NotExist) cannot be found.</DisplayMessage>
            </SVCMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new PosClient(_messageSequencingStrategy, executor);

        using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-27T12:50:52")))
        {
            var e = 
                await Assert.ThrowsAsync<OracleHospitalityClientException>(
                    () => sut.CouponIssueAsync(new AccountNumber("123"), new CouponCode("NotExist")));
            Assert.Equal("127", e.Code);
            Assert.Equal("Coupon code (NotExist) cannot be found.", e.Message);
        }
    }
}