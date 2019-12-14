using System.Threading.Tasks;
using System.Threading;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.PosOperations;

namespace Bugfree.OracleHospitality.Clients
{
    public interface IPosClient
    {
        Task<PointIssueResponse> PointIssueAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);
        Task<CouponIssueResponse> CouponIssueAsync(AccountNumber accountNumber, CouponCode couponCode, CancellationToken cancellationToken = default);
        Task<CouponInquiryResponse> CouponInquiryAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);
        Task<CouponAcceptResponse> CouponAcceptAsync(AccountNumber accountNumber, CouponCode couponCode, CancellationToken cancellationToken = default);
    }
}