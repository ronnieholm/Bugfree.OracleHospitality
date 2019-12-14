using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmOperations
{
    public class GetAccountTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public GetAccountTests()
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
                  <RequestCode>GetAccount</RequestCode>
                  <QueryCriteria conditions=""accountposref = ?"">
                    <Condition name=""accountposref"" value=""123"" />
                  </QueryCriteria>
                  <ResultSetDefinition>
                    <Column>programid</Column>
                  </ResultSetDefinition>
                </CRMMessage>";

            var request =
                new GetAccountRequest(
                    "acme.com",
                    "accountposref = ?",
                    new[] { new ColumnValue("accountposref", "123") },
                    new[] { new Column("programid") });
            var requestXml = request.BuildRequestDocument();
            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }

        [Fact]
        public async Task program_id_on_valid_account()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                    <RequestCode>GetAccount</RequestCode>
                    <ResponseCode>A</ResponseCode>
                    <ResultSet>
                        <ResultSetMetaData>
                            <RSColumn name=""programid"" type=""long"" nullable=""false""></RSColumn>
                        </ResultSetMetaData>
                        <Rows>
                            <Row id=""106632"">
                                <Col>14631</Col>
                            </Row>
                        </Rows>
                    </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual =
                await sut.GetAccountAsync(
                    "accountposref = ?",
                    new[] { new ColumnValue("accountposref", "123") },
                    new[] { new Column("programid") });

            Assert.Single(actual.Rows);
            Assert.Equal(106632, actual.Rows[0].Id);
            Assert.Equal(14631, (long)actual.Rows[0].Columns["programid"]);
        }

        [Fact]
        public async Task error_on_invalid_account()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                    <RequestCode>GetAccount</RequestCode>
                    <ResponseCode>A</ResponseCode>
                    <ResultSet>
                        <ResultSetMetaData>
                            <RSColumn name=""programid"" type=""long"" nullable=""false""></RSColumn>
                        </ResultSetMetaData>
                        <Rows></Rows>
                    </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual =
                await sut.GetAccountAsync(
                    "accountposref = ?",
                    new[] { new ColumnValue("accountposref", "123") },
                    new[] { new Column("programid") });

            Assert.Empty(actual.Rows);
        }
    }
}