using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;

namespace Bugfree.OracleHospitality.Clients
{
    // A CRM client for Oracle Hospitality, Gift & Loyalty backend. It adheres
    // to Oracle Hospitality: Gift and Loyalty CRM API (MOS Doc ID 2138811.1),
    // June 7, 2017 edition.
    public class CrmClient : ICrmClient
    {
        private readonly OracleHospitalityClientsOptions _options;
        private readonly IOracleHospitalityExecutor _executor;

        public CrmClient(IOptions<OracleHospitalityClientsOptions> options, IOracleHospitalityExecutor executor)
        {
            _options = options.Value;
            _executor = executor;
        }

        public async Task<SetCustomerResponse> SetCustomerAsync(int? rowId, ColumnValue[] dataSetColumnValues, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new SetCustomerRequest(_options.CustomerRelationshipManagementOperations.RequestSourceName, rowId, dataSetColumnValues);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new SetCustomerResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{RequestCode.Kind.SetCustomer} operation failed", e);
            }
        }

        public async Task<PostAccountTransactionResponse> PostAccountTransactionAsync(TransactionBuilder builder, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PostAccountTransactionRequest(_options.CustomerRelationshipManagementOperations.RequestSourceName, builder);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new PostAccountTransactionResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{RequestCode.Kind.PostAccountTransaction} operation failed", e);
            }
        }

        public async Task<GetColumnListResponse> GetColumnListAsync(string entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetColumnListRequest(_options.CustomerRelationshipManagementOperations.RequestSourceName, entity);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new GetColumnListResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{RequestCode.Kind.GetColumnList} operation failed", e);
            }
        }

        public async Task<GetCustomerResponse> GetCustomerAsync(string condition, ColumnValue[] columnValues, Column[] columns, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetCustomerRequest(_options.CustomerRelationshipManagementOperations.RequestSourceName, condition, columnValues, columns);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new GetCustomerResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{RequestCode.Kind.GetCustomer} operation failed", e);
            }
        }

        public async Task<GetAccountResponse> GetAccountAsync(string condition, ColumnValue[] columnValues, Column[] columns, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetAccountRequest(_options.CustomerRelationshipManagementOperations.RequestSourceName, condition, columnValues, columns);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new GetAccountResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{RequestCode.Kind.GetAccount} operation failed", e);
            }
        }

        public async Task<GetProgramResponse> GetProgramAsync(string condition, ColumnValue[] columnValues, Column[] columns, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetProgramRequest(_options.CustomerRelationshipManagementOperations.RequestSourceName, condition, columnValues, columns);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new GetProgramResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{RequestCode.Kind.GetProgram} operation failed", e);
            }
        }

        public async Task<GetCouponsResponse> GetCouponsAsync(string condition, ColumnValue[] columnValues, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetCouponsRequest(_options.CustomerRelationshipManagementOperations.RequestSourceName, condition, columnValues);
                var requestXml = request.BuildRequestDocument();
                var responseXml = await _executor.ExecuteAsync(requestXml, cancellationToken);
                return new GetCouponsResponse(requestXml, responseXml);
            }
            catch (OracleHospitalityClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException($"{RequestCode.Kind.GetCoupons} operation failed", e);
            }
        }
    }
}