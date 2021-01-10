using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmOperations
{
    public class SetCustomerTests
    {
        private readonly IOptions<OracleHospitalityClientsOptions> _options;
        private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

        public SetCustomerTests()
        {
            _options = Options.Create(A.OracleHospitalityClientOptions.Build());
            _executorLogger = new Mock<ILogger<OracleHospitalityExecutor>>().Object;
        }

        [Fact]
        public void update_existing_customer_request_generation()
        {
            const string expected = @"
                <CRMMessage language=""en_US"" currency=""DKK"">
                  <RequestSource name=""acme.com"" version=""1"" />
                  <RequestCode>SetCustomer</RequestCode>
                  <DataSet>
                    <DataSetColumns>
                      <DSColumn name=""firstname"" />
                      <DSColumn name=""lastname"" />
                    </DataSetColumns>
                    <Rows>
                      <Row id=""123"">
                        <Col>Rubber</Col>
                        <Col>Duck</Col>
                      </Row>
                    </Rows>
                  </DataSet>
                </CRMMessage>";

            var request = new SetCustomerRequest(
                "acme.com",
                123,
                new[]
                {
                    new ColumnValue("firstname", "Rubber"),
                    new ColumnValue("lastname", "Duck")
                });

            var requestXml = request.BuildRequestDocument();
            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }

        [Fact]
        public void create_new_customer_request_generation()
        {
            // Causion: a request without sortValue = 1 and PrimaryPosRef =
            // <account-number> creates a new customer that isn't associated
            // with any account. CRM API appear to not support associating the
            // customer with an account after the fact.
            const string expected = @"
                <CRMMessage language=""en_US"" currency=""DKK"">
                    <RequestSource name=""acme.com"" version=""1"" />
                    <RequestCode>SetCustomer</RequestCode>
                    <DataSet>
                        <DataSetColumns>
                            <DSColumn name=""firstName""/>
                            <DSColumn name=""lastName""/>
                            <DSColumn name=""sortValue""/>
                            <DSColumn name=""PrimaryPosRef""/>
                        </DataSetColumns>
                        <Rows>
                            <Row>
                                <Col>Jane</Col>
                                <Col>Doe</Col>
                                <Col>1</Col>
                                <Col>2200000</Col>
                            </Row>
                        </Rows>
                    </DataSet>
                </CRMMessage>";

            var request = new SetCustomerRequest(
                "acme.com",
                null,
                new[]
                {
                    new ColumnValue("firstName", "Jane"),
                    new ColumnValue("lastName", "Doe"),
                    new ColumnValue("sortValue", "1"),
                    new ColumnValue("PrimaryPosRef", "2200000")
                });

            var requestXml = request.BuildRequestDocument();
            Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
        }

        [Fact]
        public async Task create_and_update_success_response()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                  <RequestCode>SetCustomer</RequestCode>
                  <ResponseCode>A</ResponseCode>
                  <Row id=""456663"" />
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var actual = await sut.SetCustomerAsync(
                123,
                new[]
                {
                    new ColumnValue("FIRSTNAME", "Rubber"),
                    new ColumnValue("LASTNAME", "Duck")
                });

            Assert.Equal(456663, actual.RowId);
        }

        [Fact]
        public async Task update_failed_on_invalid_account()
        {
            const string response = @"
                <CRMMessage language=""en_US"" currency=""DKK"" hostversion=""1.00"">
                  <RequestCode>SetCustomer</RequestCode>
                  <ResponseCode>D</ResponseCode>
                  <DisplayMessage>com.micros.storedValue.worker.SetRollbackException: Update failed for row ID = 123</DisplayMessage>
                </CRMMessage>";

            var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
            var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
            var sut = new CrmClient(_options, executor);

            var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(
                () => 
                    sut.SetCustomerAsync(
                    123,
                    new[]
                    {
                        new ColumnValue("firstname", "Rubber"),
                        new ColumnValue("lastname", "Duck")
                    }));

            Assert.Contains("Update failed for row ID = 123", e.Message);
        }
    }
}