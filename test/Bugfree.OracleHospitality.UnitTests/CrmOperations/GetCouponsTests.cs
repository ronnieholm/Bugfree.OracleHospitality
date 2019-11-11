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
    public class GetCouponsTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public GetCouponsTests()
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
                  <RequestCode>GetCoupons</RequestCode>
                  <QueryCriteria conditions=""accountposref = ?"">
                    <Condition name=""accountposref"" value=""123"" />
                  </QueryCriteria>
                </CRMMessage>";

            var request = new GetCouponsRequest(
                "acme.com",
                "accountposref = ?",
                new[]
                {
                    new ColumnValue("accountposref", "123")
                });
            var requestXml = request.BuildRequestDocument();
            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }

        [Fact]
        public async Task error_on_invalid_account()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                  <RequestCode>GetCoupons</RequestCode>
                  <ResponseCode>A</ResponseCode>
                  <ResultSet>
                    <ResultSetMetaData>
                      <RSColumn name=""name"" type=""string"" nullable=""false""></RSColumn>
                      <RSColumn name=""description"" type=""string"" nullable=""true""></RSColumn>
                      <RSColumn name=""serialNumber"" type=""string"" nullable=""true""></RSColumn>
                      <RSColumn name=""validFromDate"" type=""timestamp"" nullable=""true""></RSColumn>
                      <RSColumn name=""validUntilDate"" type=""timestamp"" nullable=""true""></RSColumn>
                      <RSColumn name=""redeemed"" type=""boolean"" nullable=""false""></RSColumn>
                    </ResultSetMetaData>
                    <Rows></Rows>
                  </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual =
                await sut.GetCouponsAsync(
                    "accountposref = ?",
                    new[]
                    {
                        new ColumnValue("accountposref", "doNotExist")
                    });

            var metaData = actual.MetaData;
            Assert.Equal(6, metaData.Length);

            var rows = actual.Rows;
            Assert.Empty(rows);
        }

        [Fact]
        public async Task valid_account_returns_coupon_metadata()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                  <RequestCode>GetCoupons</RequestCode>
                  <ResponseCode>A</ResponseCode>
                  <ResultSet>
                    <ResultSetMetaData>
                      <RSColumn name=""name"" type=""string"" nullable=""false""></RSColumn>
                      <RSColumn name=""description"" type=""string"" nullable=""true""></RSColumn>
                      <RSColumn name=""serialNumber"" type=""string"" nullable=""true""></RSColumn>
                      <RSColumn name=""validFromDate"" type=""timestamp"" nullable=""true""></RSColumn>
                      <RSColumn name=""validUntilDate"" type=""timestamp"" nullable=""true""></RSColumn>
                      <RSColumn name=""redeemed"" type=""boolean"" nullable=""false""></RSColumn>
                    </ResultSetMetaData>
                    <Rows>
                      <Row>
                        <Col>10 DKK</Col>
                        <Col />
                        <Col>1006014</Col>
                        <Col />
                        <Col />
                        <Col>false</Col>
                      </Row>
                      <Row>
                        <Col>10 DKK</Col>
                        <Col />
                        <Col>1004019</Col>
                        <Col />
                        <Col />
                        <Col>true</Col>
                      </Row>
                    </Rows>
                  </ResultSet>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual =
                await sut.GetCouponsAsync(
                    "accountposref = ?",
                    new[]
                    {
                        new ColumnValue("accountposref", "123")
                    });

            Assert.Equal(Language.Kind.EnUs, actual.Language.Value);
            Assert.Equal(Currency.Kind.DKK, actual.Currency.Value);
            Assert.False(actual.IsTrustedSat.Value);
            Assert.Equal("1.00", actual.HostVersion.Value);
            Assert.Equal(RequestCode.Kind.GetCoupons, actual.RequestCode.Value);
            Assert.Equal(ResponseCode.Kind.Approved, actual.ResponseCode.Value);

            var metaData = actual.MetaData;
            Assert.Equal(6, metaData.Length);
            Assert.Equal("name", metaData[0].Name_.Value);
            Assert.Equal(ResultSetMetaDataColumn.Type.Kind.String, metaData[0].Type_.Value);
            Assert.False(metaData[0].Nullable_.Value);
            Assert.Equal("redeemed", metaData[5].Name_.Value);
            Assert.Equal(ResultSetMetaDataColumn.Type.Kind.Boolean, metaData[5].Type_.Value);
            Assert.False(metaData[5].Nullable_.Value);

            var rows = actual.Rows;
            Assert.Equal(2, rows.Length);

            var r0 = Assert.IsType<string>(rows[0].Columns["name"]);
            Assert.Null(rows[0].Id);
            Assert.Equal("10 DKK", r0);
            Assert.Null(rows[0].Columns["description"]);
            var r2 = Assert.IsType<string>(rows[0].Columns["serialNumber"]);
            Assert.Equal("1006014", r2);
            Assert.Null(rows[0].Columns["validFromDate"]);
            Assert.Null(rows[0].Columns["validUntilDate"]);
            var r5 = Assert.IsType<bool>(rows[0].Columns["redeemed"]);
            Assert.False(r5);
        }
    }
}