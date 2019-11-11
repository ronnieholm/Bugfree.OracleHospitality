using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

// DESIGN: this operation's response differs from other CRM operations in that
// it appears to delegate to an undocumented Oracle internal POS API call. 
// Typical to CRM operations is that their root element looks like this:
//
// <CRMMessage language=""en_US"" currency=""DKK"" isTrustedSAT=""false""
// hostversion=""1.00"">
//
// whereas this operation returns
//
// <CRMMessage hostVersion=""9.1.0000.2301"" language=""en_US"" currency=""DKK""
// isTrustedSAT=""false"">
//
// Attributes on root element is a combination of those of the CRM and POS APIs. 
// As for response elements, their names resemle those of POS operations.

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    public class PostAccountTransactionResponse : CrmResponse
    {
        public TraceId TraceId { get; private set; }
        public AccountPosRef AccountNumber { get; private set; }
        public Balance AccountBalance { get; private set; }
        public Balance LocalBalance { get; private set; }

        public override IsTrustedSat IsTrustedSat
        {
            get => _isTrustedSat;
            protected set => _isTrustedSat = value;
        }

        public override DisplayMessage DisplayMessage
        {
            get => _displayMessage;
            protected set => _displayMessage = value;
        }

        public PostAccountTransactionResponse(XE request, XE response)
            : base(request, response)
        {
        }

        public override void DeconstructHostVersionAttribute()
        {
            MapAttribute(Response_, C.hostVersion, x => HostVersion = new HostVersion(x.Value));
            ConsumeAttribute(UnconsumedResponse, C.hostVersion);
        }

        public override void DeconstructResponse()
        {
            // Contrary to other responses, this operation mirrors TraceID
            // element not below the root element, but at
            // root/Transaction/TraceId.
            ValidateElement(Request.Element(C.Transaction), Response_, C.TraceID, (a, b) =>
                new TraceId(a).ToString() == new TraceId(b).ToString());

            TraceId = new TraceId(Response_.Element(C.TraceID).Value);
            ConsumeElement(UnconsumedResponse, C.TraceID);
            AccountNumber = new AccountPosRef(Response_.Element(C.SVAN).Value);
            ConsumeElement(UnconsumedResponse, C.SVAN);

            if (ExceptionToRaiseAfterParsing != null)
            {
                // PostAccountTransaction is special in that when ResponseCode
                // is D, then isTrustedSAT attribute is present. For other
                // operations with this ResponseCode, isTrustedSAT is left out.
                MapAttribute(Response_, C.isTrustedSAT, x => IsTrustedSat = new IsTrustedSat(x.Value));
                ConsumeAttribute(UnconsumedResponse, C.isTrustedSAT);
                return;
            }

            // AccountPOSRef from request is returned as SVAN in response
            var accountPosRefElement = Request.Element(C.Transaction).Element(C.AccountPOSRef);
            var svanElement = Response_.Element(C.SVAN);
            if (new AccountPosRef(accountPosRefElement.Value).ToString() != new AccountPosRef(svanElement.Value).ToString())
                throw new OracleHospitalityClientException($"Expected element values for '{C.AccountPOSRef}' and '{C.SVAN}' to be equal. Was '{accountPosRefElement.Value}' and '{svanElement.Value}'");

            AccountBalance = new Balance(Response_.Element(C.AccountBalance).Value);
            ConsumeElement(UnconsumedResponse, C.AccountBalance);
            LocalBalance = new Balance(Response_.Element(C.LocalBalance).Value);
            ConsumeElement(UnconsumedResponse, C.LocalBalance);

            // DisplayMessage is present both in normal and error cases. Base
            // class parses it in the error case only, so we handle normal case
            // here. 
            if (ResponseCode.Value == ResponseCode.Kind.Approved)
            {
                DisplayMessage = new DisplayMessage(Response_.Element(C.DisplayMessage).Value);
                ConsumeElement(UnconsumedResponse, C.DisplayMessage);
            }
        }
    }
}