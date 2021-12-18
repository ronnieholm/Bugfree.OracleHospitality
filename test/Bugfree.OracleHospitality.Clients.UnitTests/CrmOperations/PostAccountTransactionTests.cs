using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bugfree.OracleHospitality.Clients.CrmOperations;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.UnitTests.Seedwork.TestHelpers;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;
using Type = Bugfree.OracleHospitality.Clients.CrmParselets.Transaction.Type;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmOperations;

public class PostAccountTransactionTests
{
    private readonly IOptions<OracleHospitalityClientsOptions> _options;
    private readonly ILogger<OracleHospitalityExecutor> _executorLogger;
    private CloseReopenTransactionBuilder _transaction;

    public PostAccountTransactionTests()
    {
        _options = Options.Create(A.OracleHospitalityClientOptions.Build());
        _executorLogger = new Mock<ILogger<OracleHospitalityExecutor>>().Object;
        _transaction =
            new CloseReopenTransactionBuilder()
                .WithType(Type.Kind.ReopenAccount)
                .WithCustomerFriendlyDescription("Updated by test")
                .WithProgramCode("EMPDISC")
                .WithAccountPosRef("2200005")
                .WithTraceId("Ark70tecHQrIBCXrS81XW");
    }

    [Fact]
    public void request_generation()
    {
        const string expected = @"
            <CRMMessage language=""en_US"" currency=""DKK"">
                <RequestSource name=""acme.com"" version=""1"" />
                <RequestCode>PostAccountTransaction</RequestCode>
                <Transaction>
                    <Type>6</Type>
                    <CustFriendlyDesc>Updated by test</CustFriendlyDesc>
                    <ProgramCode>EMPDISC</ProgramCode>
                    <AccountPOSRef>2200005</AccountPOSRef>
                    <TransDateTime>2019-10-08 11:11:01.8</TransDateTime>
                    <BusinessDate>2019-10-08</BusinessDate>
                    <CardPresent>0</CardPresent>
                    <LocalCurrencyISOCode>DKK</LocalCurrencyISOCode>
                    <TraceID>Ark70tecHQrIBCXrS81XW</TraceID>
                </Transaction>
            </CRMMessage>";

        _transaction = _transaction
            .WithTransactionDateTime(new DateTime(2019, 10, 8, 11, 11, 1, 800))
            .WithBusinessDate(new DateTime(2019, 10, 08));

        var request = new PostAccountTransactionRequest("acme.com", _transaction);
        var requestXml = request.BuildRequestDocument();
        Assert.Equal(XE.Parse(expected).ToString(), requestXml.ToString());
    }

    [Fact]
    public async Task close_open_account_allowed()
    {
        const string response = @"
            <CRMMessage hostVersion=""9.1.0000.2301"" language=""en_US"" currency=""DKK"" isTrustedSAT=""false"">
                <RequestCode>PostAccountTransaction</RequestCode>
                <TraceID>Ark70tecHQrIBCXrS81XW</TraceID>
                <SVAN>2200005</SVAN>
                <ResponseCode>A</ResponseCode>
                <AccountBalance>0.00</AccountBalance>
                <LocalBalance>0.00</LocalBalance>
                <DisplayMessage>Account 2200005 has been closed</DisplayMessage>
            </CRMMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new CrmClient(_options, executor);

        var actual = await sut.PostAccountTransactionAsync(_transaction);
        Assert.Equal("Account 2200005 has been closed", actual.DisplayMessage.Value);
    }

    [Fact]
    public async Task reopen_closed_account_allowed()
    {
        const string response = @"
            <CRMMessage hostVersion=""9.1.0000.2301"" language=""en_US"" currency=""DKK"" isTrustedSAT=""false"">
                <RequestCode>PostAccountTransaction</RequestCode>
                <TraceID>Ark70tecHQrIBCXrS81XW</TraceID>
                <SVAN>2200005</SVAN>
                <ResponseCode>A</ResponseCode>
                <AccountBalance>0.00</AccountBalance>
                <LocalBalance>0.00</LocalBalance>
                <DisplayMessage>Account 2200005 has been re-opened</DisplayMessage>
            </CRMMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new CrmClient(_options, executor);

        var actual = await sut.PostAccountTransactionAsync(_transaction);
        Assert.Equal("Account 2200005 has been re-opened", actual.DisplayMessage.Value);
    }

    [Fact]
    public async Task invalid_account_returns_error()
    {
        const string response = @"
            <CRMMessage hostVersion=""9.1.0000.2301"" language=""en_US"" currency=""DKK"" isTrustedSAT=""false"">
                <RequestCode>PostAccountTransaction</RequestCode>
                <TraceID>Ark70tecHQrIBCXrS81XW</TraceID>
                <SVAN>2200006</SVAN>
                <ResponseCode>D</ResponseCode>
                <DisplayMessage>This account has not been issued.</DisplayMessage>
            </CRMMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new CrmClient(_options, executor);

        var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(
            () => sut.PostAccountTransactionAsync(_transaction));
        Assert.Equal("This account has not been issued.", e.Message);
    }

    [Fact]
    public async Task close_account_already_closed_disallowed()
    {
        const string response = @"
            <CRMMessage hostVersion=""9.1.0000.2301"" language=""en_US"" currency=""DKK"" isTrustedSAT=""false"">
                <RequestCode>PostAccountTransaction</RequestCode>
                <TraceID>Ark70tecHQrIBCXrS81XW</TraceID>
                <SVAN>2200005</SVAN>
                <ResponseCode>D</ResponseCode>
                <DisplayMessage>This account is set inactive and cannot be used</DisplayMessage>
            </CRMMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new CrmClient(_options, executor);

        var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(
            () => sut.PostAccountTransactionAsync(_transaction));
        Assert.Equal("This account is set inactive and cannot be used", e.Message);
    }

    [Fact]
    public async Task reopen_account_already_open_disallowed()
    {
        const string response = @"
            <CRMMessage hostVersion=""9.1.0000.2301"" language=""en_US"" currency=""DKK"" isTrustedSAT=""false"">
                <RequestCode>PostAccountTransaction</RequestCode>
                <TraceID>Ark70tecHQrIBCXrS81XW</TraceID>
                <SVAN>2200005</SVAN>
                <ResponseCode>D</ResponseCode>
                <DisplayMessage>This account cannot be used, its status is invalid</DisplayMessage>
            </CRMMessage>";

        var handler = CreateMockMessageHandler(HttpStatusCode.OK, CreateSoapResponse(response.Trim()));
        var executor = new OracleHospitalityExecutor(_options, _executorLogger, new HttpClient(handler));
        var sut = new CrmClient(_options, executor);

        var e = await Assert.ThrowsAsync<OracleHospitalityClientException>(
            () => sut.PostAccountTransactionAsync(_transaction));
        Assert.Equal("This account cannot be used, its status is invalid", e.Message);
    }
}