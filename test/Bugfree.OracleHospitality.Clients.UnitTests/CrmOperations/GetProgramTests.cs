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
    public class GetProgramTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public GetProgramTests()
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
                  <RequestCode>GetProgram</RequestCode>
                  <QueryCriteria conditions=""programid = ?"">
                    <Condition name=""programid"" value=""123"" />
                  </QueryCriteria>
                  <ResultSetDefinition>
                    <Column>programcode</Column>
                  </ResultSetDefinition>
                </CRMMessage>";

            var request =
                new GetProgramRequest(
                    "acme.com",
                    "programid = ?",
                    new[] { new ColumnValue("programid", "123") },
                    new[] { new Column("programcode") });
            var requestXml = request.BuildRequestDocument();
            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }

        [Fact]
        public async Task program_name_on_valid_programid()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                    <RequestCode>GetProgram</RequestCode>
                    <ResponseCode>A</ResponseCode>
                    <ResultSet>
                        <ResultSetMetaData>
                        <RSColumn name=""programcode"" type=""string"" nullable=""true""></RSColumn>
                        </ResultSetMetaData>
                        <Rows>
                        <Row id=""123"">
                            <Col>EMPDISC</Col>
                        </Row>
                        </Rows>
                    </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual =
                await sut.GetProgramAsync(
                    "programid = ?",
                    new[] { new ColumnValue("programid", "123") },
                    new[] { new Column("programcode") });

            Assert.Single(actual.Rows);
            Assert.Equal(123, actual.Rows[0].Id);
            Assert.Equal("EMPDISC", (string)actual.Rows[0].Columns["programcode"]);
        }

        [Fact]
        public async Task error_on_invalid_programid()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                    <RequestCode>GetProgram</RequestCode>
                    <ResponseCode>A</ResponseCode>
                    <ResultSet>
                        <ResultSetMetaData>
                        <RSColumn name=""programcode"" type=""string"" nullable=""true""></RSColumn>
                        </ResultSetMetaData>
                        <Rows></Rows>
                    </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual =
                await sut.GetProgramAsync(
                    "programid = ?",
                    new[] { new ColumnValue("programid", "123") },
                    new[] { new Column("programcode") });

            Assert.Empty(actual.Rows);
        }
    }
}