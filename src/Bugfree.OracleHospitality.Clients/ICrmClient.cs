using System.Threading;
using System.Threading.Tasks;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;

namespace Bugfree.OracleHospitality.Clients;

public interface ICrmClient
{
    Task<SetCustomerResponse> SetCustomerAsync(int? rowId, ColumnValue[] columnValues, CancellationToken cancellationToken = default);
    Task<PostAccountTransactionResponse> PostAccountTransactionAsync(TransactionBuilder builder, CancellationToken cancellationToken = default);
    Task<GetColumnListResponse> GetColumnListAsync(string entity, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse> GetCustomerAsync(string condition, ColumnValue[] columnValues, Column[] columns, CancellationToken cancellationToken = default);
    Task<GetAccountResponse> GetAccountAsync(string condition, ColumnValue[] columnValues, Column[] columns, CancellationToken cancellationToken = default);
    Task<GetProgramResponse> GetProgramAsync(string condition, ColumnValue[] columnValues, Column[] columns, CancellationToken cancellationToken = default);
    Task<GetCouponsResponse> GetCouponsAsync(string condition, ColumnValue[] columnValues, CancellationToken cancellationToken = default);
}