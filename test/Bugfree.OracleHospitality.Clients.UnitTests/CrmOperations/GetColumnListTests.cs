using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmOperations;

public class GetColumnListTests
{
    private readonly IOptions<OracleHospitalityClientsOptions> _options;
    private readonly ILogger<OracleHospitalityExecutor> _executorLogger;

    public GetColumnListTests()
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
                <RequestCode>GetColumnList</RequestCode>
                <QueryCriteria request=""customer"" />
            </CRMMessage>";

        var request = new GetColumnListRequest("acme.com", "customer");
        var requestXml = request.BuildRequestDocument();
        Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
    }

    [Fact]
    public async Task error_on_invalid_entity()
    {
        const string response = @"
            <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                <RequestCode>GetColumnList</RequestCode>
                <ResponseCode>E</ResponseCode>
                <Error code=""1"">Unsupported parameter: NonExistingEntity</Error>
            </CRMMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new CrmClient(_options, executor);

        var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(() =>
            sut.GetColumnListAsync("NonExistingEntity"));
        Assert.Equal("1", e.Code);
    }

    [Fact]
    public async Task valid_entity_returns_column_metadata()
    {
        const string response = @"
            <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false"" hostversion=""1.00"">
                <RequestCode>GetColumnList</RequestCode>
                <ResponseCode>A</ResponseCode>
                <ResultSet>
                    <Rows>
                        <Row>
                            <Col type=""A10"">PREFIX</Col>
                            <Col type=""N19"">PARENTCACUSTOMERID</Col>
                            <Col type=""A25"">MOBILEPHONENUMBER</Col>
                            <Col type=""N1"">MEETINGPLANNER</Col>
                            <Col type=""A50"">USERIDNUMBER</Col>
                            <Col type=""A32"">LATITUDE</Col>
                            <Col type=""A32"">MIDDLENAME</Col>
                            <Col type=""Timestamp"">SIGNUPDATE</Col>
                            <Col type=""N1"">EMAILCHNLDELIVERABLE</Col>
                            <Col type=""N5"">PRIMARYPOSREFSTATUS</Col>
                            <Col type=""Timestamp"">LASTTRANSACTIONBUSINESSDATE</Col>
                            <Col type=""N1"">MAILCHNLDELIVERABLE</Col>
                            <Col type=""A50"">MISCFIELD1</Col>
                            <Col type=""Timestamp"">ANNIVERSARYDATE</Col>
                            <Col type=""Decimal"">DISTANCE</Col>
                            <Col type=""A50"">SOURCECODE</Col>
                            <Col type=""A32"">LASTUPDATEDBY</Col>
                            <Col type=""A50"">MISCFIELD4</Col>
                            <Col type=""A10"">SUFFIX</Col>
                            <Col type=""A50"">MISCFIELD3</Col>
                            <Col type=""A50"">MISCFIELD2</Col>
                            <Col type=""N10"">SIGNUPSOURCEID</Col>
                            <Col type=""A25"">WORKPHONENUMBER</Col>
                            <Col type=""Timestamp"">REISSUENEWCUSTKITDATE</Col>
                            <Col type=""N19"">SORTVALUE</Col>
                            <Col type=""A32"">STOREEXTREF</Col>
                            <Col type=""A50"">PASSWORDHINT</Col>
                            <Col type=""N10"">DATASOURCEID</Col>
                            <Col type=""Timestamp"">LASTUPDATED</Col>
                            <Col type=""N1"">OTHERCHNLDELIVERABLE</Col>
                            <Col type=""N1"">ISCLEANED</Col>
                            <Col type=""A50"">USERNAME</Col>
                            <Col type=""A50"">CITY</Col>
                            <Col type=""A25"">FAXNUMBER</Col>
                            <Col type=""N1"">ACTIVE</Col>
                            <Col type=""N5"">EMAILCHNLSTATUS</Col>
                            <Col type=""A50"">LASTNAME</Col>
                            <Col type=""A32"">STATE</Col>
                            <Col type=""Decimal"">USERNUMERIC1</Col>
                            <Col type=""N5"">HOMEPHONECHNLSTATUS</Col>
                            <Col type=""A76"">ADDRESSLINE1</Col>
                            <Col type=""A76"">ADDRESSLINE2</Col>
                            <Col type=""Timestamp"">REPLACECARDDATE</Col>
                            <Col type=""N10"">PARENTRELATIONSHIP</Col>
                            <Col type=""N19"">CACUSTOMERID</Col>
                            <Col type=""A50"">CAMPAIGN</Col>
                            <Col type=""Timestamp"">FIRSTTRANSACTIONBUSINESSDATE</Col>
                            <Col type=""N10"">REISSUENEWCUSTKITCOUNTER</Col>
                            <Col type=""A16"">OTHERPHONENUMBER</Col>
                            <Col type=""A50"">FIRSTNAME</Col>
                            <Col type=""A16"">POSTALCODE</Col>
                            <Col type=""A50"">COUNTY</Col>
                            <Col type=""N10"">HASHCODE</Col>
                            <Col type=""N5"">OTHERCHNLSTATUS</Col>
                            <Col type=""N1"">ISORGANIZATION</Col>
                            <Col type=""N1"">CONCIERGE</Col>
                            <Col type=""N19"">KID</Col>
                            <Col type=""Decimal"">USERNUMERIC2</Col>
                            <Col type=""N19"">SIGNUPLOCATIONID</Col>
                            <Col type=""A150"">PASSWORD</Col>
                            <Col type=""N10"">REPLACECARDCOUNTER</Col>
                            <Col type=""A50"">EMAILADDRESS</Col>
                            <Col type=""N1"">WELCOMEEMAILSENT</Col>
                            <Col type=""Timestamp"">BIRTHDAY</Col>
                            <Col type=""A50"">COUNTRY</Col>
                            <Col type=""Timestamp"">CREATEDDATE</Col>
                            <Col type=""N1"">USEFORTESTEMAIL</Col>
                            <Col type=""N1"">HOMEPHONECHNLDELIVERABLE</Col>
                            <Col type=""A32"">PRIMARYPOSREF</Col>
                            <Col type=""A25"">HOMEPHONENUMBER</Col>
                            <Col type=""N3"">BIRTHMONTH</Col>
                            <Col type=""A1"">GENDER</Col>
                            <Col type=""N1"">ISDEDUPED</Col>
                            <Col type=""A32"">LONGITUDE</Col>
                            <Col type=""N3"">BIRTHDAYOFMONTH</Col>
                            <Col type=""A50"">USERALPHA1</Col>
                            <Col type=""N5"">MAILCHNLSTATUS</Col>
                            <Col type=""A50"">USERALPHA2</Col>
                            <Col type=""A50"">ORGANIZATIONNAME</Col>
                            <Col type=""A10"">PMACODE</Col>
                        </Row>
                    </Rows>
                </ResultSet>
            </CRMMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new CrmClient(_options, executor);

        var actual = await sut.GetColumnListAsync("customer");
        var columns = actual.Row.Columns;
        Assert.Equal(80, columns.Count);

        // Oracle backend uses all caps for fields. That's a direct
        // translation of field names from GL's MS SQL Server database. In
        // queries both upper and lower case may be used as MS SQL Server is
        // generally case insensitive.
        Assert.Equal("A10", columns["PREFIX"]);
        Assert.Equal("N1", columns["ACTIVE"]);
    }
}