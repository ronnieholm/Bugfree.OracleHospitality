using System;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class ResponseCode : IResponseElement
    {
        public enum Kind
        {
            None,
            Approved,
            DataCenterInitiatedError
        }

        public string HostCode { get; }
        public Kind Value { get; }

        // If an error occurs and response code isn't "Approved", form of
        // response is 
        //
        // <ResponseCode hostCode="20">D</ResponseCode>
        // <DisplayMessage>Missing SVAN Element</DisplayMessage>
        //
        // A successful response code has the form <ResponseCode>A</ResponseCode>
        // and sometimes, but not always, contains DisplayMessage element.
        public ResponseCode(XE responseCode)
        {
            // POS API spec, Page 19: according to DisplayMessage documentation,
            // additional response codes include E, V, and P. Those should be
            // shown to POS terminal user. We haven't encounted these codes and
            // thus doesn't parse those.
            var value = responseCode.Value;
            FieldTypes.AssertA1(value);

            if (value == "A")
                Value = Kind.Approved;
            else if (value == "D")
            {
                var hostCodeAttribute = ExpectAttribute(responseCode, C.hostCode);
                HostCode = hostCodeAttribute.Value;
                Value = Kind.DataCenterInitiatedError;
            }
            else
                throw new ArgumentException($"Unsupported {nameof(Kind)} value: '{value}'");
        }

        public ResponseCode(Kind kind)
        {
            if (kind == Kind.None)
                throw new ArgumentException($"{nameof(Kind)} must not be {kind}");
            Value = kind;
        }

        public override string ToString()
        {
            return Value switch
            {
                Kind.Approved => "A",
                Kind.DataCenterInitiatedError => "D",
                _ => throw new ArgumentException($"Unsupported {nameof(Kind)}: '{Value}'"),
            };
        }
    }
}