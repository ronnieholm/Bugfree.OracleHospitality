using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.Seedwork;
using Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;
using Bugfree.OracleHospitality.Clients.PosOperations;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosOperations
{
    public class PointIssueTests
    {
        readonly IOptions<OracleHospitalityClientsOptions> _options;
        readonly IPosMessageSequencingStrategy _messageSequencingStrategy;
        readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public PointIssueTests()
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
                var request = new PointIssueRequest(
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
                        <RequestCode>POINT_ISSUE</RequestCode>
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
        public async Task loyalty_account_created()
        {
            const string response = @"
                <SVCMessage hostVersion=""9.1.0000.2301"" 
                            version=""1"" 
                            posIntfcName=""posInterfaceName"" 
                            posIntfcVersion=""1.00"" 
                            language=""en-US"" 
                            currency=""DKK"" 
                            sequence=""0"" 
                            retransmit=""n"">
                    <RequestCode>POINT_ISSUE</RequestCode>
                    <TraceID>190826145135N000000</TraceID>
                    <Amount>0.00</Amount>
                    <SVAN>2200003</SVAN>
                    <ItemType>D</ItemType>
                    <ItemNumber>103</ItemNumber>
                    <AccountCurrency>DKK</AccountCurrency>
                    <AccountBalance>0.00</AccountBalance>
                    <LocalBalance>0.00</LocalBalance>
                    <ExchangeRate>1.00</ExchangeRate>
                    <ResponseCode>A</ResponseCode>
                    <ProgramCode>EMPDISC</ProgramCode>
                    <ProgramName>Employee Discount 20%</ProgramName>
                    <PointsIssued>0</PointsIssued>
                    <BonusPointsIssued>0</BonusPointsIssued>
                    <DisplayMessage>Transaction Complete. </DisplayMessage>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
            {
                var actual = await sut.PointIssueAsync(new AccountNumber("2200003"));
                Assert.Equal(ItemType.Kind.Discount, actual.ItemType.Value);
                Assert.Equal(103, actual.ItemNumber.Value);
                Assert.Equal(Currency.Kind.DKK, actual.AccountCurrency.Value);
                Assert.Equal(0.00m, actual.AccountBalance.Value);
                Assert.Equal(0.00m, actual.LocalBalance.Value);
                Assert.Equal(1.00m, actual.ExchangeRate.Value);
                Assert.Equal("EMPDISC", actual.ProgramCode.Value);
                Assert.Equal("Employee Discount 20%", actual.ProgramName.Value);
                Assert.Equal(0m, actual.PointsIssued.Value);
                Assert.Equal(0m, actual.BonusPointsIssued.Value);
                Assert.Null(actual.PrintLines);
                Assert.Equal("Transaction Complete. ", actual.DisplayMessage.Value);
            }
        }

        [Fact]
        public async Task loyalty_account_created_with_rule_awards_pos_print_text_non_empty()
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
                <RequestCode>POINT_ISSUE</RequestCode>
                <TraceID>190826145135N000000</TraceID>
                <Amount>0</Amount>
                <SVAN>luxo7wt7mrm8rj4t0j24twi3</SVAN>
                <ItemType>T</ItemType>
                <ItemNumber>1234</ItemNumber>
                <AccountCurrency>DKK</AccountCurrency>
                <AccountBalance>0.00</AccountBalance>
                <LocalBalance>0.00</LocalBalance>
                <ExchangeRate>1.00</ExchangeRate>
                <ResponseCode>A</ResponseCode>
                <ProgramCode>lux</ProgramCode>
                <ProgramName>lux</ProgramName>
                <PointsIssued>0</PointsIssued>
                <BonusPointsIssued>0</BonusPointsIssued>
                <PrintLines>
                    <PrintLine>LUX FORDEL</PrintLine>
                </PrintLines>
                <DisplayMessage>Transaction Complete. </DisplayMessage>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
            {
                var actual = await sut.PointIssueAsync(new AccountNumber("luxo7wt7mrm8rj4t0j24twi3"));
                Assert.Equal(ItemType.Kind.Tender, actual.ItemType.Value);
                Assert.Equal(1234, actual.ItemNumber.Value);
                Assert.Equal(Currency.Kind.DKK, actual.AccountCurrency.Value);
                Assert.Equal(0.00m, actual.AccountBalance.Value);
                Assert.Equal(0.00m, actual.LocalBalance.Value);
                Assert.Equal(1.00m, actual.ExchangeRate.Value);
                Assert.Equal("lux", actual.ProgramCode.Value);
                Assert.Equal("lux", actual.ProgramName.Value);
                Assert.Equal(0m, actual.PointsIssued.Value);
                Assert.Equal(0m, actual.BonusPointsIssued.Value);
                Assert.Single(actual.PrintLines.Values);
                Assert.Equal("LUX FORDEL", actual.PrintLines.Values[0].Value);
                Assert.Equal("Transaction Complete. ", actual.DisplayMessage.Value);
            }
        }

        [Fact]
        public async Task account_already_exists_and_open()
        {
            const string response = @"
                <SVCMessage hostVersion=""9.1.0000.2301"" 
                            version=""1"" 
                            posIntfcName=""posInterfaceName"" 
                            posIntfcVersion=""1.00"" 
                            language=""en-US"" 
                            currency=""DKK"" 
                            sequence=""0"" 
                            retransmit=""n"">
                    <RequestCode>POINT_ISSUE</RequestCode>
                    <TraceID>190829140000N000000</TraceID>
                    <Amount>0</Amount>
                    <SVAN>2200003</SVAN>
                    <ResponseCode hostCode=""70"">D</ResponseCode>
                    <DisplayMessage>This card is already on this check</DisplayMessage>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-29T14:00:00")))
            {
                var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() =>
                    sut.PointIssueAsync(new AccountNumber("2200003")));
                Assert.Equal("70", e.Code);
                Assert.Equal("This card is already on this check", e.Message);
            }
        }

        [Fact]
        public async Task account_already_exists_and_closed()
        {
            const string response = @"
                <SVCMessage language=""en-US"" 
                            retransmit=""n""
                            sequence=""00""
                            currency=""DKK"" 
                            posIntfcVersion=""1.00"" 
                            posIntfcName=""posInterfaceName"" 
                            version=""1"" 
                            hostVersion=""9.1.0000.2301"">
                    <RequestCode>POINT_ISSUE</RequestCode>
                    <TraceID>190829140000N000000</TraceID>
                    <Amount>0</Amount>
                    <SVAN>2200000</SVAN>
                    <ResponseCode hostCode=""106"">D</ResponseCode>
                    <DisplayMessage>This account cannot be used, it is closed</DisplayMessage>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-29T14:00:00")))
            {
                var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() =>
                    sut.PointIssueAsync(new AccountNumber("2200000")));
                Assert.Equal("106", e.Code);
                Assert.Equal("This account cannot be used, it is closed", e.Message);
            }
        }
    }
}