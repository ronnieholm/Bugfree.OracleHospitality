using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XE = System.Xml.Linq.XElement;

// DESIGN: generating a SOAP proxy for CRM/POS API WSDL would cause us to lose
// control over the underlying socket. Internally WCF uses WebClient and not the
// newer HttpClient. With WebClient we have little control over connection
// re-use and no way to prevent ephemeral port exhaustion.
//
// The CRM/POS API WSDL define SOAP envelope only and not payload. We don't have
// much use for the thin 150 lines of auto-generated service wrapper as we'd
// still have to construct the request and parse the response by hand.
//
// This class must be thread safe. Because when a client makes parallel calls
// through PosClient or CrmClient it leads to parallel calls through the
// executor. Even though during dependency injection, the executor is registered
// as transient, because there may be only one shared PosClient or CrmClient
// instance at the call site, there may be only one executor for each. If the
// executor wasn't thread safe, we'd end up with one thread calling an operation
// like GetCustomer on account A and another thread calling GetCustomer on
// account B. In between calls to ExecuteAsync below, which would store in0 in a
// field, another call to ExecuteAsync could come in, resetting in0. When the
// first call then builds the request, it would be with the wrong in0, and so
// request and response validation would end up failing.

namespace Bugfree.OracleHospitality.Clients
{
    public class OracleHospitalityExecutor : IOracleHospitalityExecutor
    {
        // Only select methods on HttpClient are thread safe. Don't call any
        // from this class that aren't on the positive list:
        // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?redirectedfrom=MSDN&view=netcore-2.1#remarks
        private readonly HttpClient _httpClient;
        private readonly ILogger<OracleHospitalityExecutor> _logger;
        private readonly OracleHospitalityClientsOptions _options;

        private const string SoapRequestBodyTemplate = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ejb=""ejb.storedValue.micros.com"">
    <soapenv:Body>
        <ejb:processRequest>
            <in0>{0}</in0>
            <in1>{1}</in1>
            <in2>{2}</in2>
            <in3>{3}</in3>
        </ejb:processRequest>
    </soapenv:Body>
</soapenv:Envelope>";

        public OracleHospitalityExecutor(IOptions<OracleHospitalityClientsOptions> options, ILogger<OracleHospitalityExecutor> logger, HttpClient httpClient)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            _options = options.Value;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<XE> ExecuteAsync(XE in0, CancellationToken cancellationToken)
        {
            _logger.LogInformation(in0.ToString());

            var soapRequestBody = BuildSoapXml(in0);
            var soapResponse = await SendSoapMessageAsync(soapRequestBody, cancellationToken);
            var returnElement = DeconstructSoapResponse(soapResponse);
            _logger.LogInformation(returnElement.ToString());
            return returnElement;
        }

        private async Task<XE> SendSoapMessageAsync(XE soapRequestBody, CancellationToken cancellationToken)
        {
            var content = new StringContent(soapRequestBody.ToString(), Encoding.UTF8, "text/xml");
            var response = await _httpClient.PostAsync(_options.StoredValueServiceUrl, content, cancellationToken);
            var responseBodyString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new OracleHospitalityClientException($"Unable to POST to {_options.StoredValueServiceUrl}. Expected {(int)HttpStatusCode.OK} {HttpStatusCode.OK}. Got {(int)response.StatusCode} {response.StatusCode} with body '{responseBodyString}'");

            return XE.Parse(responseBodyString);
        }

        private XE BuildSoapXml(XE /* request */ in0)
        {
            // Canonicalize XML by removing redundant whitespace and newlines.
            // Canonicalizing by way of XElement.ToString() has the effect of
            // removing trailing "<?xml version="1.0" encoding="UTF-8"?>. We
            // could add it back in, but it doesn't make a different to the
            // Oracle backend.
            var in0Canonicalized = in0.ToString(SaveOptions.DisableFormatting);

            // Because we're storing the request XML element inside the Soap XML
            // element, we must encode the request XML. .NET Core supports using
            // HTML encoding only when XML encoding, but because XML and HTML
            // differs slightly (HTML doesn't encode newlines whereas in XML they
            // must be encoded as ';&#xD;'), we disabled formatting during
            // request XML canonicalization, causing the XML to not include
            // newlines. Otherwise, when the Oracle backend calculates CRC32 of
            // the request XML, it'll not match and respond with
            //
            //   <ResponseCode>D</ResponseCode> <DisplayMessage>Request
            //   failed integrity check</DisplayMessage> 
            var in0Encoded = WebUtility.HtmlEncode(in0Canonicalized);
            var in3 = Crc32.ToPaddedCrc32String(Crc32.Compute(Encoding.UTF8.GetBytes(in0Canonicalized)));
            return XE.Parse(
                string.Format(
                    SoapRequestBodyTemplate,
                    in0Encoded,
                    /* in1 */ _options.LogonName,
                    /* in2 */ _options.Password,
                    /* CRC32 of in0 */ in3));
        }

        private XE DeconstructSoapResponse(XE soapResponse)
        {
            // SOAP envelope responses come back looking like so (here return
            // element is yet to be decoded):
            //
            // <S:Envelope xmlns:S="http://schemas.xmlsoap.org/soap/envelope/">
            //   <S:Body>
            //     <ns2:processRequestResponse xmlns:ns2="ejb.storedValue.micros.com">
            //       <return>
            //         &lt;?xml version="1.0" encoding="UTF-8"?&gt;&lt;SVCMessage hostVersion="9.1.0000.2301" version="1" posIntfcName="posInterfaceName" posIntfcVersion="1.00" language="en-US" currency="DKK" sequence="00" retransmit="n"&gt;&lt;RequestCode&gt;COUPON_INQUIRY&lt;/RequestCode&gt;&lt;TraceID&gt;190725135831N000000&lt;/TraceID&gt;&lt;Amount&gt;0.00&lt;/Amount&gt;&lt;SVAN&gt;123&lt;/SVAN&gt;&lt;AccountCurrency&gt;DKK&lt;/AccountCurrency&gt;&lt;ExchangeRate&gt;1.00&lt;/ExchangeRate&gt;&lt;ResponseCode&gt;A&lt;/ResponseCode&gt;&lt;Actions&gt;&lt;Action&gt;&lt;Type tid="12"&gt;Accept Coupon&lt;/Type&gt;&lt;Data pid="9"&gt;1004019&lt;/Data&gt;&lt;Code&gt;coffee&lt;/Code&gt;&lt;Text&gt;Coupon: Coffee, Always Valid&lt;/Text&gt;&lt;/Action&gt;&lt;/Actions&gt;&lt;PrintLines /&gt;&lt;DisplayMessage&gt;There is an eligible coupon for this card.&lt;/DisplayMessage&gt;&lt;/SVCMessage&gt;
            //       </return>
            //     </ns2:processRequestResponse>
            //   </S:Body>
            // </S:Envelope>
            XNamespace s = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns2 = "ejb.storedValue.micros.com";

            var bodyElement = soapResponse.Element(s + "Body");
            if (bodyElement == null)
                throw new OracleHospitalityClientException("Expected element 'Body' in SOAP response body");

            var processRequestResponseElement = bodyElement.Element(ns2 + "processRequestResponse");
            if (processRequestResponseElement == null)
                throw new OracleHospitalityClientException("Expected element 'processRequestResponse' in SOAP response body");

            var returnElement = processRequestResponseElement.Element("return");
            if (returnElement == null)
                throw new OracleHospitalityClientException("Expected element 'return' in SOAP response body");

            var returnElementValueString = returnElement.Value;
            if (string.IsNullOrWhiteSpace(returnElementValueString))
                throw new OracleHospitalityClientException("Expect operation response body inside SOAP response body");

            try
            {
                return XE.Parse(returnElementValueString);
            }
            catch (Exception e)
            {
                throw new OracleHospitalityClientException("Unable to parse operation response body", e);
            }
        }
    }
}