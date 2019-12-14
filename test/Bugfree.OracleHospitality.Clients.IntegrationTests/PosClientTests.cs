using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies;
using static Bugfree.OracleHospitality.Clients.IntegrationTests.ConfigurationHelpers;

namespace Bugfree.OracleHospitality.Clients.IntegrationTests
{
    public class PosClientTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly IPosMessageSequencingStrategy _messageSequencingStrategy;
        private readonly IOracleHospitalityExecutor _executor;

        public PosClientTests()
        {
            _options = ParseConfiguration();
            _messageSequencingStrategy = new TerminalIdOnlyStrategy(_options);
            _executor = new OracleHospitalityExecutor(_options, new NullLogger<OracleHospitalityExecutor>(), new HttpClient());
        }

        [Fact]
        public async Task end_to_end()
        {
            // To create a new account, specify a non-existing account number.
            // Creating an account isn't part of this test because (1) accounts
            // can never be deleted and (2) we'd have to determine next
            // available account number prior to creating the account.
            var accountNumber = new AccountNumber("2200000");
            var client = new PosClient(_messageSequencingStrategy, _executor);

            var pointIssue = await client.PointIssueAsync(accountNumber);
            var couponInquiry = await client.CouponInquiryAsync(pointIssue.AccountNumber);
            var couponCode = new CouponCode("10DKK");
            var couponIssue = await client.CouponIssueAsync(accountNumber, couponCode);
            var couponAccept = await client.CouponAcceptAsync(accountNumber, couponIssue.CouponCode);
        }
    }
}