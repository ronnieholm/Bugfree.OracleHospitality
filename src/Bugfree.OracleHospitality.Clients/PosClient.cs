using System;
using System.Threading.Tasks;
using System.Threading;
using Bugfree.OracleHospitality.Clients.Seedwork;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.PosOperations;

namespace Bugfree.OracleHospitality.Clients
{
    // A POS terminal emulator for Oracle Hospitality, Gift & Loyalty backend.
    // It adheres to Oracle Hospitality: Gift and Loyalty POS Web Services API
    // specification at https://docs.oracle.com/cd/E80526_01/doc.91/e91264.pdf,
    // Release 8.5.1, September 2019 edition.
    public class PosClient : IPosClient
    {
        private readonly IPosMessageSequencingStrategy _messageSequencingStrategy;
        private readonly IOracleHospitalityExecutor _executor;

        public PosClient(IPosMessageSequencingStrategy messageSequencingStrategy, IOracleHospitalityExecutor executor)
        {
            _messageSequencingStrategy = messageSequencingStrategy;
            _executor = executor;
        }

        public async Task<PointIssueResponse> PointIssueAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageId = _messageSequencingStrategy.Next();
                var request =
                    new PointIssueRequest(
                        TimeProvider.Now,
                        messageId,
                        accountNumber);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new PointIssueResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{TransactionKind.POINT_ISSUE} operation failed", e);
            }
        }

        public async Task<CouponIssueResponse> CouponIssueAsync(AccountNumber accountNumber, CouponCode couponCode, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageId = _messageSequencingStrategy.Next();
                var request =
                    new CouponIssueRequest(
                        TimeProvider.Now,
                        messageId,
                        accountNumber,
                        couponCode);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new CouponIssueResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{TransactionKind.SV_ISSUE_COUPON} operation failed", e);
            }
        }

        public async Task<CouponInquiryResponse> CouponInquiryAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageId = _messageSequencingStrategy.Next();
                var request =
                    new CouponInquiryRequest(
                        TimeProvider.Now,
                        messageId,
                        accountNumber);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new CouponInquiryResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{TransactionKind.COUPON_INQUIRY} operation failed", e);
            }
        }

        public async Task<CouponAcceptResponse> CouponAcceptAsync(AccountNumber accountNumber, CouponCode couponCode, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageId = _messageSequencingStrategy.Next();
                var request =
                    new CouponAcceptRequest(
                        TimeProvider.Now,
                        messageId,
                        accountNumber,
                        couponCode);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new CouponAcceptResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{TransactionKind.SV_ACCEPT_COUPON} operation failed", e);
            }
        }
    }
}