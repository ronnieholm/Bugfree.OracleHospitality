using System;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    public abstract class CrmResponse
    {
        // Attributes
        public Language Language { get; private set; }
        public Currency Currency { get; private set; }

        // Virtual because PostAccountTransactionResponse overrides default
        // behavior.
        protected IsTrustedSat _isTrustedSat;
        public virtual IsTrustedSat IsTrustedSat
        {
            get
            {
                if (ResponseCode.Value == ResponseCode.Kind.DataCenterInitiatedError)
                    throw new OracleHospitalityClientException($"Property only supports reading when {nameof(ResponseCode)} is different from {nameof(ResponseCode.Kind.DataCenterInitiatedError)}");
                return _isTrustedSat;
            }

            protected set => _isTrustedSat = value;
        }

        // PostAccountTransactionResponse overrides default HostVersion parsing.
        // Contrary to other CRM operations, its response has this attribute
        // lowercased. Inside GL Web UI, if we navigate to any account against
        // which we ran this operation, then the account's transactions, we see
        // this operation actually had RequestCode SV_CRM_POST_TRANS. This
        // indicates an undocumented POS API operation for Oracle internal use.
        public HostVersion HostVersion { get; protected set; }

        // Elements
        public RequestCode RequestCode { get; private set; }
        public ResponseCode ResponseCode { get; private set; }

        protected OracleHospitalityClientException ExceptionToRaiseAfterParsing { get; private set; }

        private Error _error;
        public Error Error
        {
            get
            {
                if (ResponseCode.Value != ResponseCode.Kind.Error)
                    throw new OracleHospitalityClientException($"Property only supports reading when {nameof(ResponseCode)} is {nameof(ResponseCode.Kind.Error)}. Was {ResponseCode.Value}");
                return _error;
            }
            protected set => _error = value;
        }

        // PostAccountTransactionResponse overrides default behavior which is
        // why property is virtual. Because this operation is probably the
        // undocumented SV_CRM_POST_TRANS POS API operation behind the scenes,
        // its response contains a DisplayMessage in both success and failure
        // cases. Other CRM operations includes DisplayMessage only on the error
        // path.
        protected DisplayMessage _displayMessage;
        public virtual DisplayMessage DisplayMessage
        {
            get
            {
                if (ResponseCode.Value != ResponseCode.Kind.DataCenterInitiatedError)
                    throw new OracleHospitalityClientException($"Property only supports reading when {nameof(ResponseCode)} is {nameof(ResponseCode.Kind.DataCenterInitiatedError)}. Was {ResponseCode.Value}");
                return _displayMessage;
            }
            protected set => _displayMessage = value;
        }

        protected XE UnconsumedResponse { get; }
        protected XE Request { get; }
        protected XE Response_ { get; }

        protected abstract void DeconstructResponse();

        private static readonly (string, Func<string, string, bool>)[] AttributesSharedAcrossRequestAndResponse = new (string, Func<string, string, bool>)[]
        {
            (C.language, (a, b) => new Language(a).ToString() == new Language(b).ToString()),
            (C.currency, (a, b) => new Currency(a).ToString() == new Currency(b).ToString()),
        };

        private static readonly (string, Func<string, string, bool>)[] ElementsSharedAcrossRequestAndResponse = new (string, Func<string, string, bool>)[]
        {
            (C.RequestCode, (a, b) => new RequestCode(a).ToString() == new RequestCode(b).ToString())
        };

        public CrmResponse(XE request, XE response)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response_ = response ?? throw new ArgumentNullException(nameof(response));
            UnconsumedResponse = XE.Parse(Response_.ToString());

            ValidateBaseDocumentResponse();
            DeconstructBaseDocumentResponse();
            DeconstructHostVersionAttribute();
            DeconstructResponse();
            ValidateConsumedResponse();

            if (ExceptionToRaiseAfterParsing != null)
                throw ExceptionToRaiseAfterParsing;
        }

        public virtual void DeconstructHostVersionAttribute()
        {
            MapAttribute(Response_, C.hostversion, x => HostVersion = new HostVersion(x.Value));
            ConsumeAttribute(UnconsumedResponse, C.hostversion);
        }

        private void ValidateBaseDocumentResponse()
        {
            var responseCodeElement = ExpectElement(Response_, C.ResponseCode);
            ResponseCode = new ResponseCode(responseCodeElement.Value);
            ConsumeElement(UnconsumedResponse, C.ResponseCode);

            if (ResponseCode.Kind.Approved == ResponseCode.Value)
            {
                // Intentionally left empty
            }
            else if (ResponseCode.Kind.Error == ResponseCode.Value)
            {
                var errorElement = ExpectElement(Response_, C.Error);
                Error = new Error(errorElement);
                ConsumeElement(UnconsumedResponse, C.Error);
                ExceptionToRaiseAfterParsing = new OracleHospitalityClientException(Error.Code, Error.Message);
            }
            else if (ResponseCode.Kind.DataCenterInitiatedError == ResponseCode.Value)
            {
                // Parses elements:
                // <ResponseCode>D</ResponseCode>
                // <DisplayMessage>com.micros.storedValue.worker.SetRollbackException: Update failed for row ID = 123</DisplayMessage>
                var displayMessage = ExpectElement(Response_, C.DisplayMessage);
                DisplayMessage = new DisplayMessage(displayMessage.Value);
                ConsumeElement(UnconsumedResponse, C.DisplayMessage);
                ExceptionToRaiseAfterParsing = new OracleHospitalityClientException(DisplayMessage.Value);
            }
            else
                ExceptionToRaiseAfterParsing = new OracleHospitalityClientException($"Unhandled response code: {ResponseCode.Value}");

            foreach (var (attributeName, comparatorFn) in AttributesSharedAcrossRequestAndResponse)
                ValidateAttribute(Request, Response_, attributeName, comparatorFn);

            foreach (var (elementName, comparatorFn) in ElementsSharedAcrossRequestAndResponse)
                ValidateElement(Request, Response_, elementName, comparatorFn);
        }

        private void DeconstructBaseDocumentResponse()
        {
            var attributeMapping = new (string, Action<XA>)[]
            {
                (C.language, x => Language = new Language(x.Value)),
                (C.currency, x => Currency = new Currency(x.Value))
            };
            foreach (var (attributeName, creatorFn) in attributeMapping)
            {
                MapAttribute(Response_, attributeName, creatorFn);
                ConsumeAttribute(UnconsumedResponse, attributeName);
            }

            var elementMapping = new (string, Action<XE>)[]
            {
                (C.RequestCode, x => RequestCode = new RequestCode(x.Value))
            };
            foreach (var (elementName, creatorFn) in elementMapping)
            {
                MapElement(Response_, elementName, creatorFn);
                ConsumeElement(UnconsumedResponse, elementName);
            }

            if (ResponseCode.Value != ResponseCode.Kind.DataCenterInitiatedError)
            {
                MapAttribute(Response_, C.isTrustedSAT, x => IsTrustedSat = new IsTrustedSat(x.Value));
                ConsumeAttribute(UnconsumedResponse, C.isTrustedSAT);
            }
        }

        private void ValidateConsumedResponse()
        {
            if (UnconsumedResponse.HasElements)
                throw new InvalidOperationException($"Expected every attribute of response to have been consumed: {UnconsumedResponse}");
            if (UnconsumedResponse.HasAttributes)
                throw new InvalidOperationException($"Expected every element of response to have been consumed: {UnconsumedResponse}");
        }
    }
}