using System;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Version = Bugfree.OracleHospitality.Clients.PosParselets.Version;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.PosOperations;

public abstract class PosResponse
{
    // Attributes
    public SequenceNumber SequenceNumber { get; private set; }
    public Language Language { get; private set; }
    public Transmission Transmission { get; private set; }
    public Currency Currency { get; private set; }
    public PosInterfaceVersion PosInterfaceVersion { get; private set; }
    public PosInterfaceName PosInterfaceName { get; private set; }
    public Version Version { get; private set; }
    public HostVersion HostVersion { get; private set; }

    // Elements
    public RequestCode RequestCode { get; private set; }
    public TraceId TraceId { get; private set; }
    public Amount Amount { get; private set; }
    public ResponseCode ResponseCode { get; private set; }
    public DisplayMessage DisplayMessage { get; private set; }
    public AccountNumber AccountNumber { get; private set; }

    protected XE UnconsumedResponse { get; }
    protected XE Request { get; }
    protected XE Response_ { get; }

    protected OracleHospitalityClientException ExceptionToRaiseAfterParsing { get; private set; }

    public virtual void AssertSvanElement()
    {
        ValidateElement(Request, Response_, C.SVAN, (a, b) => new AccountNumber(a).ToString() == new AccountNumber(b).ToString());
    }

    public abstract void DeconstructResponse();

    private static readonly (string, Func<string, string, bool>)[] AttributesSharedAcrossRequestAndResponse = 
    {
        (C.sequence, (a, b) => new SequenceNumber(a).ToString() == new SequenceNumber(b).ToString()),
        (C.language, (a, b) => new Language(a).ToString() == new Language(b).ToString()),
        (C.retransmit, (a, b) => new Transmission(a).ToString() == new Transmission(b).ToString()),
        (C.currency, (a, b) => new Currency(a).ToString() == new Currency(b).ToString()),
        (C.posIntfcVersion, (a, b) => new PosInterfaceVersion(a).ToString() == new PosInterfaceVersion(b).ToString()),
        (C.posIntfcName, (a, b) => new PosInterfaceName(a).ToString() == new PosInterfaceName(b).ToString()),
        (C.version, (a, b) => new Version(a).ToString() == new Version(b).ToString())
    };

    private static readonly (string, Func<string, string, bool>)[] ElementsSharedAcrossRequestAndResponse =
    {
        (C.RequestCode, (a, b) => new RequestCode(a).ToString() == new RequestCode(b).ToString()),
        (C.TraceID, (a, b) => new TraceId(a).ToString() == new TraceId(b).ToString()),
        (C.Amount, (a, b) => new Amount(a).ToString() == new Amount(b).ToString())
    };

    public PosResponse(XE request, XE response)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Response_ = response ?? throw new ArgumentNullException(nameof(response));
        UnconsumedResponse = XE.Parse(Response_.ToString());
        AssertBaseDocumentResponse();
        AssertSvanElement();
        DeconstructBaseDocumentResponse();
        DeconstructResponse();
        AssertConsumedResponse();

        if (ExceptionToRaiseAfterParsing != null)
            throw ExceptionToRaiseAfterParsing;
    }

    private void AssertConsumedResponse()
    {
        // Because of the low quality of the POS API spec, client explicitly
        // marks each attribute and element of the response as processed. If
        // after parsing the response, an attribute or element remains, it
        // indicates a parsing error. This way, the client can detect Oracle
        // API changes or unexpected behavior in the wild. If such behavior
        // is encountered, a test case should be added for the response and
        // the parser adjusted to support it.
        if (UnconsumedResponse.HasElements)
            throw new InvalidOperationException($"Expected every attribute of response to have been consumed: {UnconsumedResponse}");
        if (UnconsumedResponse.HasAttributes)
            throw new InvalidOperationException($"Expected every element of response to have been consumed: {UnconsumedResponse}");
    }

    private void AssertBaseDocumentResponse()
    {
        // If an error occurs and response code isn't "Approved", form of
        // response is <ResponseCode
        // hostCode="20">D</ResponseCode><DisplayMessage>Missing SVAN
        // Element</DisplayMessage>
        var responseCodeElement = ExpectElement(Response_, C.ResponseCode);
        ResponseCode = new ResponseCode(responseCodeElement);
        ConsumeElement(UnconsumedResponse, C.ResponseCode);

        var displayMessageElement = ExpectElement(Response_, C.DisplayMessage);
        DisplayMessage = new DisplayMessage(displayMessageElement.Value);
        ConsumeElement(UnconsumedResponse, C.DisplayMessage);

        // Save off exception and throw it later when the PosResponse
        // subclass have had a chance to parse response in its
        // DeconstructResponse(). We do so because we value parsing
        // correctly over immediately raising an Oracle error or we might
        // not get to parse part of the error response.
        if (ResponseCode.Value == ResponseCode.Kind.DataCenterInitiatedError)
            ExceptionToRaiseAfterParsing = new OracleHospitalityClientException(ResponseCode.HostCode, DisplayMessage.Value);

        foreach (var (attributeName, comparatorFn) in AttributesSharedAcrossRequestAndResponse)
            ValidateAttribute(Request, Response_, attributeName, comparatorFn);

        foreach (var (elementName, comparatorFn) in ElementsSharedAcrossRequestAndResponse)
            ValidateElement(Request, Response_, elementName, comparatorFn);
    }

    private void DeconstructBaseDocumentResponse()
    {
        var attributeMapping = new (string, Action<XA>)[]
        {
            (C.sequence, x => SequenceNumber = new SequenceNumber(x.Value)),
            (C.language, x => Language = new Language(x.Value)),
            (C.retransmit, x => Transmission = new Transmission(x.Value)),
            (C.currency, x => Currency = new Currency(x.Value)),
            (C.posIntfcVersion, x => PosInterfaceVersion = new PosInterfaceVersion(x.Value)),
            (C.posIntfcName, x => PosInterfaceName = new PosInterfaceName(x.Value)),
            (C.version, x => Version = new Version(x.Value)),
            (C.hostVersion, x => HostVersion = new HostVersion(x.Value))
        };
        foreach (var (attributeName, creatorFn) in attributeMapping)
        {
            MapAttribute(Response_, attributeName, creatorFn);
            ConsumeAttribute(UnconsumedResponse, attributeName);
        }

        var elementMapping = new (string, Action<XE>)[]
        {
            (C.RequestCode, x => RequestCode = new RequestCode(x.Value)),
            (C.TraceID, x => TraceId = new TraceId(x.Value)),
            (C.Amount, x => Amount = new Amount(x.Value)),
            (C.SVAN, x => AccountNumber = new AccountNumber(x.Value))
        };
        foreach (var (elementName, creatorFn) in elementMapping)
        {
            MapElement(Response_, elementName, creatorFn);
            ConsumeElement(UnconsumedResponse, elementName);
        }
    }
}