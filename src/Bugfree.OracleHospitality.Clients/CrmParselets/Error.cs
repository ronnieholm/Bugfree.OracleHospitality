using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    // Only if ResponseCode is 'E' is the following element included in response:
    //
    // <Error code="1">Unsupported parameter: NonExistingEntity</Error>
    //
    // When ResponseCode is 'D', only the following node is included in response:
    // <DisplayMessage>com.micros.storedValue.worker.SetRollbackException: Update failed for row ID = 123</DisplayMessage>
    public class Error
    {
        public string Code { get; }
        public string Message { get; }

        public Error(XE errorElement)
        {
            var codeAttribute = ExpectAttribute(errorElement, C.code);
            Code = FieldTypes.AssertString(codeAttribute.Value);
            Message = FieldTypes.AssertString(errorElement.Value);
        }
    }
}