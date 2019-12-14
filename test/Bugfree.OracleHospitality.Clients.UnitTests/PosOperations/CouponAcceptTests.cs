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
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosOperations
{
    public class CouponAcceptTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly IPosMessageSequencingStrategy _messageSequencingStrategy;
        private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public CouponAcceptTests()
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
                var request = new CouponAcceptRequest(
                    TimeProvider.Now,
                    new MessageId(new TerminalId(0), new SequenceNumber(42), new CheckNumber(13)),
                    new AccountNumber("123"),
                    new CouponCode("987"));
                var requestXml = request.BuildRequestDocument();

                const string expected = @"
                    <SVCMessage version=""1"" 
                                posIntfcName=""posInterfaceName"" 
                                posIntfcVersion=""1.00"" 
                                language=""en-US"" 
                                currency=""DKK"" 
                                sequence=""42"" 
                                retransmit=""n"">
                        <RequestCode>SV_ACCEPT_COUPON</RequestCode>
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
                        <CouponCode>987</CouponCode>
                    </SVCMessage>";

                Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
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
                            sequence=""0"" 
                            retransmit=""n"">
                    <RequestCode>SV_ACCEPT_COUPON</RequestCode>
                    <TraceID>190826145135N000000</TraceID>
                    <Amount>0.00</Amount>
                    <SVAN>123</SVAN>
                    <ResponseCode hostCode=""93"">D</ResponseCode>
                    <DisplayMessage>Coupon (NotExist) cannot be found</DisplayMessage>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
            {
                var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() => 
                    sut.CouponAcceptAsync(new AccountNumber("123"), new CouponCode("NotExist")));
                Assert.Equal("93", e.Code);
                Assert.Equal("Coupon (NotExist) cannot be found", e.Message);
            }
        }

        [Fact]
        public async Task valid_coupon_code()
        {
            // For SV_ACCEPT_COUPON response, SVAN changes meaning from
            // mirroring the request SVAN to holding the issued CouponCode.
            const string response = @"
                <SVCMessage hostVersion=""9.1.0000.2301"" 
                            version=""1"" 
                            posIntfcName=""posInterfaceName"" 
                            posIntfcVersion=""1.00"" 
                            language=""en-US"" 
                            currency=""DKK"" 
                            sequence=""00""
                            retransmit=""n"">
                    <RequestCode>SV_ACCEPT_COUPON</RequestCode>
                    <TraceID>190826145135N000000</TraceID>
                    <Amount>0.00</Amount>
                    <SVAN>1005016</SVAN>
                    <ItemType>D</ItemType>
                    <ItemNumber>150</ItemNumber>
                    <ResponseCode>A</ResponseCode>
                    <DisplayMessage>10 DKK Coupon accepted</DisplayMessage>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
            {
                var actual = await sut.CouponAcceptAsync(new AccountNumber("doesNotMatter"), new CouponCode("1005016"));
                Assert.Equal(ItemType.Kind.Discount, actual.ItemType.Value);
                Assert.Equal(150, actual.ItemNumber.Value);
                Assert.Equal("10 DKK Coupon accepted", actual.DisplayMessage.Value);
                Assert.Equal("1005016", actual.AccountNumber.Value);
            }
        }

        [Fact]
        public async Task coupon_code_already_accepted()
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
                    <RequestCode>SV_ACCEPT_COUPON</RequestCode>
                    <TraceID>190826145135N000000</TraceID>
                    <Amount>0.00</Amount>
                    <SVAN>123</SVAN>
                    <ResponseCode hostCode=""86"">D</ResponseCode>
                    <DisplayMessage>This coupon had already been used</DisplayMessage>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
            {
                var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() =>
                    sut.CouponAcceptAsync(new AccountNumber("123"), new CouponCode("1005016")));
                Assert.Equal("86", e.Code);
                Assert.Equal("This coupon had already been used", e.Message);
            }
        }
    }
}