using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using K = Bugfree.OracleHospitality.Clients.CrmParselets.ResultSetMetaDataColumn.Type.Kind;
using static Bugfree.OracleHospitality.Clients.IntegrationTests.ConfigurationHelpers;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;
using Type = Bugfree.OracleHospitality.Clients.CrmParselets.Transaction.Type;

namespace Bugfree.OracleHospitality.Clients.IntegrationTests
{
    public class CrmClientTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly IOracleHospitalityExecutor _executor;
        private readonly Dictionary<K, List<string>> MetaDataTypeFieldTypeMapping = new Dictionary<K, List<string>>();

        public CrmClientTests()
        {
            _options = ParseConfiguration();
            _executor = new OracleHospitalityExecutor(_options, new NullLogger<OracleHospitalityExecutor>(), new HttpClient());
        }

        private void InferMetaDataToFieldSpecifications(ResultSetMetaDataColumn[] metaData, Dictionary<string, string> columns)
        {
            // Infers relationship between Ax, Nx, ... returned by GetColumnList
            // and column types of string, bool, int, short, ...  returned by
            // GetCustomer. It's unclear why Oracle uses two notations for data
            // types. Nevertheless, knowing the relationship comes in handy in
            // implementing the mapper inside the Row class, responsible for
            // mapping GetCustomer response column to .NET types. Sizes of .NET
            // doesn't necessarily align with Ax, Nx, ... or string, bool, int,
            // ... but rather should be treated as a third kind of data types.
            var uniqueTypes = metaData.Select(md => md.Type_.Value).Distinct().ToArray();
            var mappings = new Dictionary<K, List<string>>();
            foreach (var t in uniqueTypes)
            {
                var metaDataColumns = metaData.Where(md => md.Type_.Value == t).ToArray();
                var fields = new List<string>();
                foreach (var c in metaDataColumns)
                    fields.Add(columns.Single(c2 => c2.Key == c.Name_.Value).Value);
                fields = fields.Distinct().OrderBy(f => f).ToList();
                mappings.Add(t, fields);
            }

            foreach (var kv in mappings)
            {
                if (!MetaDataTypeFieldTypeMapping.ContainsKey(kv.Key))
                    MetaDataTypeFieldTypeMapping[kv.Key] = new List<string>();
                MetaDataTypeFieldTypeMapping[kv.Key].AddRange(kv.Value);
            }
        }

        private async Task<GetCustomerResponse> GetCustomer(CrmClient client, string accountNumber)
        {
            // Whereas this operations returns A25 as the value of a column,
            // when we actually issue queries using GetCustomer, that isn't the
            // column type returned. Instead its string, long, boolean in
            // concert with a nullable attribute.
            var columnList = await client.GetColumnListAsync("customer");

            // By including every column in the query, we make sure that our
            // mapper keeps supporting every known type.
            var columnNames = columnList.Row.Columns.Select(c => new Column(c.Key)).ToArray();
            var customer = await client.GetCustomerAsync(
                "primaryposref = ?",
                new[] { new ColumnValue("primaryposref", accountNumber) },
                columnNames);
            InferMetaDataToFieldSpecifications(customer.MetaData, columnList.Row.Columns);

            Assert.Single(customer.Rows);
            Assert.Equal(accountNumber, customer.Rows[0].Columns["PRIMARYPOSREF"]);
            return customer;
        }

        private async Task<GetAccountResponse> GetAccount(CrmClient client, string accountNumber)
        {
            var columnList = await client.GetColumnListAsync("account");
            var columnNames = columnList.Row.Columns.Select(c => new Column(c.Key)).ToArray();
            var account = await client.GetAccountAsync(
                "accountposref = ?",
                new[] { new ColumnValue("accountposref", accountNumber) },
                columnNames);
            InferMetaDataToFieldSpecifications(account.MetaData, columnList.Row.Columns);

            Assert.Single(account.Rows);
            Assert.Equal(accountNumber, account.Rows[0].Columns["ACCOUNTPOSREF"]);
            return account;
        }

        private async Task<GetProgramResponse> GetProgram(CrmClient client, GetAccountResponse account)
        {
            var columnList = await client.GetColumnListAsync("program");
            var columnNames = columnList.Row.Columns.Select(c => new Column(c.Key)).ToArray();
            var programId = account.Rows[0].Columns["PROGRAMID"].ToString();
            var program = await client.GetProgramAsync(
                "programid = ?",
                new[] { new ColumnValue("programid", programId) },
                columnNames);
            InferMetaDataToFieldSpecifications(program.MetaData, columnList.Row.Columns);

            Assert.Single(program.Rows);
            Assert.Equal("EMPDISC", program.Rows[0].Columns["PROGRAMCODE"]);
            return program;
        }

        private async Task<GetCouponsResponse> GetCoupons(CrmClient client, string accountNumber)
        {
            return await client.GetCouponsAsync(
                "accountposref = ?",
                new[]
                {
                    new ColumnValue("accountposref", accountNumber)
                });
        }

        private async Task<PostAccountTransactionResponse> FlipAccountState(CrmClient client, GetCustomerResponse customer, GetProgramResponse program)
        {
            var accountNumber = (string)customer.Rows[0].Columns["PRIMARYPOSREF"];
            var accountState = (bool)customer.Rows[0].Columns["ACTIVE"];
            var programCode = (string)program.Rows[0].Columns["PROGRAMCODE"];

            var kind = accountState ? Type.Kind.CloseAccount : Type.Kind.ReopenAccount;
            var transaction =
                new CloseReopenTransactionBuilder()
                    .WithType(kind)
                    .WithCustomerFriendlyDescription("Updated by test")
                    .WithProgramCode(programCode)
                    .WithAccountPosRef(accountNumber);
            return await client.PostAccountTransactionAsync(transaction);
        }

        private async Task<SetCustomerResponse> SetCustomer(CrmClient client, GetCustomerResponse customer)
        {
            var now = DateTime.Now;
            var accountNumber = (string)customer.Rows[0].Columns["PRIMARYPOSREF"];
            var setCustomer = await client.SetCustomerAsync(customer.Rows.Single().Id, new[]
            {
                new ColumnValue("firstname", "Rubber"),
                new ColumnValue("lastname", now.ToString()),
            });

            var getCustomerUpdated = await GetCustomer(client, accountNumber);
            var lastname = (string)getCustomerUpdated.Rows.Single().Columns["LASTNAME"];

            // UNDOCUMENTED: updating account to ACTIVE = false doesn't cause
            // the SetCustomer API call to fail or the account state to change.
            // Oracle Hospitality carries out the update and returns Approved
            // response code to client. But the web UI remains frozen in that it
            // doesn't show updated values, but remains stuck on the value at
            // the time of closing. Only clicking an account will show the
            // updated name (likely a bug in Web UI). In order to change the
            // account state, one must use the PostAccountTransaction
            // transaction. The ACTIVE attribute value is likely replicated from
            // somewhere else by the backend and therefore any updated to it are
            // ignored.
            Assert.Equal(getCustomerUpdated.Rows[0].Id, setCustomer.RowId);
            Assert.Equal(
                new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second),
                DateTime.Parse(lastname));

            return setCustomer;
        }

        [Fact]
        public async Task end_to_end()
        {
            var client = new CrmClient(_options, _executor);
            const string AccountNumber = "2200005";

            var getCustomer = await GetCustomer(client, AccountNumber);
            var getAccount = await GetAccount(client, AccountNumber);
            var getProgram = await GetProgram(client, getAccount);
            var getCoupons = await GetCoupons(client, AccountNumber);
            var setCustomer = await SetCustomer(client, getCustomer);
            var postAccountTransaction = await FlipAccountState(client, getCustomer, getProgram);

            var mappings = MetaDataTypeFieldTypeMapping.Select(kv =>
                new KeyValuePair<K, string>(
                    kv.Key,
                    string.Join(',', kv.Value.Distinct().OrderBy(s => s))))
                .ToDictionary(i => i.Key, i => i.Value);

            // Ensure Oracle doesn't change any field types which might
            // invalidate our mapping to .NET types.
            var kinds = Enum.GetValues(typeof(K));
            Assert.Equal(((K[])kinds).Except(new[] { K.None }).Count(), mappings.Count);
            Assert.Equal("N1", mappings[K.Boolean]);
            Assert.Equal("N19", mappings[K.Long]);
            Assert.Equal("A1,A10,A150,A16,A200,A25,A3,A32,A4,A50,A76", mappings[K.String]);
            Assert.Equal("Timestamp", mappings[K.Timestamp]);
            Assert.Equal("N3,N5", mappings[K.Short]);
            Assert.Equal("Decimal", mappings[K.Double]);
            Assert.Equal("N10", mappings[K.Int]);
        }

        [Fact(Skip = "Don't include in every test run as it accumulates data that cannot be deleted")]
        public async Task SetCustomerCreateNewCustomerAndAssociateWithExistingAccountTest()
        {
            var client = new CrmClient(_options, _executor);
            var _ = await client.SetCustomerAsync(null, new[]
            {
                new ColumnValue("firstname", "Rubber"),
                new ColumnValue("lastname", "Duck"),
                new ColumnValue("sortvalue", "1"),
                new ColumnValue("primaryposref", "2200000")
            });
        }

        [Fact(Skip = "Don't include in every test run as it might loop for too long")]
        public async Task triggers_http_406_on_too_many_calls()
        {
            var i = 1;
            try
            {
                var client = new CrmClient(_options, _executor);
                var columnValues = new[] { new ColumnValue("primaryposref", "2200000") };
                var columns = new[] { new Column("firstname"), new Column("lastname"), new Column("active") };
                for (; ; i++)
                {
                    var _ = await client.GetCustomerAsync("primaryposref = ?", columnValues, columns);
                }
            }
            catch (OracleHospitalityClientException)
            {
                // Intentionally left blank. Query i for count.
            }
        }
    }
}