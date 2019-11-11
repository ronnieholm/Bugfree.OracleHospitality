using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmOperations
{
    public class GetCustomerTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public GetCustomerTests()
        {
            _options = Options.Create(A.OracleHospitalityClientOptions.Build());
            _executorLogger = new Mock<ILogger<OracleHospitalityExecutor>>().Object;
        }

        [Fact]
        public void request_generation()
        {
            const string expected = @"
                <CRMMessage language=""en_US"" currency=""DKK"">
                  <RequestSource name=""acme.com"" version=""1"" />
                  <RequestCode>GetCustomer</RequestCode>
                  <QueryCriteria conditions=""primaryposref = ?"">
                    <Condition name=""primaryposref"" value=""123"" />
                  </QueryCriteria>
                  <ResultSetDefinition>
                    <Column>firstname</Column>
                    <Column>lastname</Column>
                  </ResultSetDefinition>
                </CRMMessage>";

            var request = new GetCustomerRequest(
                "acme.com",
                "primaryposref = ?",
                new[] { new ColumnValue("primaryposref", "123") },
                new[]
                {
                    new Column("firstname"),
                    new Column("lastname")
                });

            var requestXml = request.BuildRequestDocument();
            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }

        [Fact]
        public async Task invalid_account_returns_empty_row_set()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                  <RequestCode>GetCustomer</RequestCode>
                  <ResponseCode>A</ResponseCode>
                  <ResultSet>
                    <ResultSetMetaData>
                      <RSColumn name=""firstname"" type=""string"" nullable=""true""></RSColumn>
                      <RSColumn name=""lastname"" type=""string"" nullable=""true""></RSColumn>
                    </ResultSetMetaData>
                    <Rows></Rows>
                  </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual = await sut.GetCustomerAsync(
                "primaryposref = ?",
                new[] { new ColumnValue("primaryPOSRef", "doesNotMatter") },
                new[] { new Column("firstname"), new Column("lastname") });

            Assert.Equal(2, actual.MetaData.Length);
            Assert.Empty(actual.Rows);
        }

        [Fact]
        public async Task valid_account_with_single_customer()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                  <RequestCode>GetCustomer</RequestCode>
                  <ResponseCode>A</ResponseCode>
                  <ResultSet>
                    <ResultSetMetaData>
                      <RSColumn name=""firstname"" type=""string"" nullable=""true""></RSColumn>
                      <RSColumn name=""lastname"" type=""string"" nullable=""true""></RSColumn>
                    </ResultSetMetaData>
                    <Rows>
                      <Row id=""27350"">
                        <Col>George</Col>
                        <Col>Washington</Col>
                      </Row>
                    </Rows>
                  </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual = await sut.GetCustomerAsync(
                "primaryposref = ?",
                new[] { new ColumnValue("primaryposref", "doesNotMatter") },
                new[] { new Column("firstname"), new Column("lastname") });

            Assert.Equal(2, actual.MetaData.Length);
            Assert.Single(actual.Rows);

            var r0 = actual.Rows[0];
            Assert.Equal(27350, r0.Id);
            Assert.Equal("George", r0.Columns["firstname"]);
            Assert.Equal("Washington", r0.Columns["lastname"]);
        }

        [Fact]
        public async Task valid_account_with_multiple_customers()
        {
            // If we create multiple customers and associate each with the same
            // account, response becomes as below.
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                  <RequestCode>GetCustomer</RequestCode>
                  <ResponseCode>A</ResponseCode>
                  <ResultSet>
                    <ResultSetMetaData>
                      <RSColumn name=""firstname"" type=""string"" nullable=""true""></RSColumn>
                      <RSColumn name=""lastname"" type=""string"" nullable=""true""></RSColumn>
                    </ResultSetMetaData>
                    <Rows>
                      <Row id=""416440"">
                        <Col>John</Col>
                        <Col>Doe</Col>
                      </Row>
                      <Row id=""422056"">
                        <Col />
                        <Col />
                      </Row>
                      <Row id=""456663"">
                        <Col>Rubber</Col>
                        <Col>Duck</Col>
                      </Row>
                    </Rows>
                  </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual = await sut.GetCustomerAsync(
                "primaryposref = ?",
                new[] { new ColumnValue("primaryPOSRef", "doesNotMatter") },
                new[] { new Column("firstname"), new Column("lastname") });

            Assert.Equal(3, actual.Rows.Length);

            var r0 = actual.Rows[0];
            Assert.Equal(416440, r0.Id);
            Assert.Equal("John", r0.Columns["firstname"]);
            Assert.Equal("Doe", r0.Columns["lastname"]);

            var r1 = actual.Rows[1];
            Assert.Equal(422056, r1.Id);
            Assert.Null(r1.Columns["firstname"]);
            Assert.Null(r1.Columns["lastname"]);

            var r2 = actual.Rows[2];
            Assert.Equal(456663, r2.Id);
            Assert.Equal("Rubber", r2.Columns["firstname"]);
            Assert.Equal("Duck", r2.Columns["lastname"]);
        }
    }
}