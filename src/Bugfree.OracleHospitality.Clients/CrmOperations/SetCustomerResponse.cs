using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    public class SetCustomerResponse : CrmResponse
    {
        public int RowId { get; private set; }

        public SetCustomerResponse(XE request, XE response)
            : base(request, response)
        {
        }

        public override void DeconstructResponse()
        {
            if (ExceptionToRaiseAfterParsing != null)
                return;

            var rowIdElement = ExpectElement(Response_, C.Row);
            var idAttribute = ExpectAttribute(rowIdElement, C.id);
            ConsumeElement(UnconsumedResponse, C.Row);
            RowId = int.Parse(idAttribute.Value);
        }
    }
}