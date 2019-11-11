using System;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    public abstract class CrmRequest
    {
        protected string RequestSourceName { get; }

        public abstract XE /* In0 */ BuildRequestDocument();

        protected CrmRequest(string requestSourceName)
        {
            if (string.IsNullOrWhiteSpace(requestSourceName))
                throw new ArgumentException(nameof(requestSourceName));
            RequestSourceName = requestSourceName;
        }

        protected XE BuildBaseDocument(RequestCode.Kind requestCode)
        {
            // UNDOCUMENTED: base document for CRM requests. Compared to the POS
            // API spec, the concept of a base document isn't explicitly part of
            // the CRM API spec and has been inferred from examples. For reasons
            // unknown, GetCoupons API spec example doesn't include
            // RequestSourceName. It probably supports it given that other
            // operations does so we include RequestSourceName for every
            // operation.
            return new XE(C.CRMMessage,
                new XA(C.language, new Language(Language.Kind.EnUs)),
                new XA(C.currency, new Currency(Currency.Kind.DKK)),
                new XE(C.RequestSource, new XA(C.name, RequestSourceName), new XA(C.version, "1")),
                new XE(C.RequestCode, requestCode));
        }
    }
}