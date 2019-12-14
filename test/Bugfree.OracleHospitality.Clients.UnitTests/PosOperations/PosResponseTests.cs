using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosOperations
{
    public class PosResponseTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly IPosMessageSequencingStrategy _messageSequencingStrategy;
        private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public PosResponseTests()
        {
            _options = Options.Create(
                A.OracleHospitalityClientOptions
                .WithTerminalIdLowerBound(0)
                .WithTerminalIdUpperBound(100).Build());
            _messageSequencingStrategy = FixedStrategy.Default();
            _executorLogger = new Mock<ILogger<OracleHospitalityExecutor>>().Object;
        }

        [Fact]
        public async Task report_unconsumed_attribute()
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
                            retransmit=""n""
                            newAttribute=""42"">
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
                var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() =>
                    sut.CouponInquiryAsync(new AccountNumber("42")));
                Assert.Contains(@"<SVCMessage newAttribute=""42"" />", e.InnerException.Message);
            }
        }

        [Fact]
        public async Task report_unconsumed_element()
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
                    <NewElement>42</NewElement>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
            {
                var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() =>
                    sut.CouponInquiryAsync(new AccountNumber("42")));
                Assert.Contains(
                    "<SVCMessage><NewElement>42</NewElement></SVCMessage>",
                    e.InnerException.Message.Replace(Environment.NewLine, "").Replace(" ", ""));
            }
        }

        [Fact]
        public async Task with_both_unconsumed_element_and_backend_reporting_error_unconsumed_exception_takes_precedence()
        {
            // Only check ResponseCode and throw exception after validating
            // every element and attribute. Otherwise, we miss the opportunity
            // to catch responses of unanticipated forms.
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
                    <ResponseCode hostCode=""93"">D</ResponseCode>
                    <DisplayMessage>Coupon (NotExist) cannot be found</DisplayMessage>
                    <NewElement>42</NewElement>
                </SVCMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new PosClient(_messageSequencingStrategy, executor);

            using (new TimeProviderTestScope(() => DateTime.Parse("2019-08-26T14:51:35")))
            {
                var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() =>
                    sut.CouponInquiryAsync(new AccountNumber("42")));
                Assert.Contains(
                    "<SVCMessage><NewElement>42</NewElement></SVCMessage>", 
                    e.InnerException.Message.Replace(Environment.NewLine, "").Replace(" ", ""));
            }
        }
    }
}