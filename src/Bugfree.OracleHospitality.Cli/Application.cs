using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Bugfree.OracleHospitality.Clients;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using Bugfree.OracleHospitality.Clients.PosParselets;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;

namespace Bugfree.OracleHospitality.Cli
{
    public class Application
    {
        private readonly ILogger<Application> _logger;
        private readonly IPosClient _posClient;
        private readonly ICrmClient _crmClient;

        public Application(ILogger<Application> logger, IPosClient posClient, ICrmClient crmClient)
        {
            _logger = logger;
            _posClient = posClient;
            _crmClient = crmClient;
        }

        private Column[] CollectColumns(IEnumerable<string> columns) => columns.Select(c => new Column(c)).ToArray();

        public async Task RunAsync(PointIssueOptions o) =>
            await _posClient.PointIssueAsync(new AccountNumber(o.AccountNumber));

        public async Task RunAsync(CouponIssueOptions o) =>
            await _posClient.CouponIssueAsync(new AccountNumber(o.AccountNumber), new CouponCode(o.CouponCode));

        public async Task RunAsync(CouponInquiryOptions o) =>
            await _posClient.CouponInquiryAsync(new AccountNumber(o.AccountNumber));

        public async Task RunAsync(CouponAcceptOptions o) =>
            await _posClient.CouponAcceptAsync(new AccountNumber(o.AccountNumber), new CouponCode(o.CouponCode));

        public async Task RunAsync(SetCustomerOptions o)
        {
            var columnValues = CollectColumnValues(o.ColumnValues);
            await _crmClient.SetCustomerAsync(o.RowId, columnValues);
        }

        public async Task RunAsync(PostAccountTransactionOptions o)
        {
            var transaction =
                new CloseReopenTransactionBuilder()
                    .WithType(o.Type)
                    .WithCustomerFriendlyDescription($"Updated by {Assembly.GetExecutingAssembly().GetName().Name}")
                    .WithProgramCode(o.ProgramCode)
                    .WithAccountPosRef(o.AccountPosRef);
            await _crmClient.PostAccountTransactionAsync(transaction);
        }

        public async Task RunAsync(GetColumnListOptions o) =>
            await _crmClient.GetColumnListAsync(o.Request);

        private ColumnValue[] CollectColumnValues(IEnumerable<string> keyValueArguments)
        {
            var keyValues = keyValueArguments.ToArray();
            if (keyValues.Length % 2 != 0)
                throw new Exception($"{nameof(keyValueArguments)} keys/values argument must have an equal number of keys/values");

            var result = new ColumnValue[keyValues.Length / 2];
            for (var i = 0; i < result.Length; i++)
                result[i] = new ColumnValue(keyValues[2 * i], keyValues[2 * i + 1]);
            return result;
        }

        public async Task RunAsync(GetCustomerOptions o)
        {
            var columnValues = CollectColumnValues(o.ColumnValues);
            var columns = CollectColumns(o.Columns);
            await _crmClient.GetCustomerAsync(o.Conditions, columnValues, columns);
        }

        public async Task RunAsync(GetAccountOptions o)
        {
            var columnValues = CollectColumnValues(o.ColumnValues);
            var columns = CollectColumns(o.Columns);
            await _crmClient.GetAccountAsync(o.Conditions, columnValues, columns);
        }

        public async Task RunAsync(GetProgramOptions o)
        {
            var columnValues = CollectColumnValues(o.ColumnValues);
            var columns = CollectColumns(o.Columns);
            await _crmClient.GetProgramAsync(o.Conditions, columnValues, columns);
        }

        public async Task RunAsync(GetCouponsOptions o)
        {
            var columnValues = CollectColumnValues(o.ColumnValues);
            await _crmClient.GetCouponsAsync(o.Conditions, columnValues);
        }

        public async Task RunAsync(GuidToAccountNumberOptions o)
        {
            var number = ConversionHelpers.GuidToAccountNumber(o.Guid);
            _logger.LogInformation(number);
            await Task.CompletedTask;
        }
    }
}