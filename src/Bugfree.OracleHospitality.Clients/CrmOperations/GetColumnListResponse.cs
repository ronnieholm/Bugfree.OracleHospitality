using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    public class GetColumnListResponse : CrmResponse
    {
        public GetColumnListResultSet.Row Row { get; private set; }

        public GetColumnListResponse(XE request, XE response)
            : base(request, response)
        {
        }

        public override void DeconstructResponse()
        {
            if (ExceptionToRaiseAfterParsing != null)
                return;

            var resultSetElement = ExpectElement(Response_, C.ResultSet);
            var resultSet = new GetColumnListResultSet(resultSetElement);
            ConsumeElement(UnconsumedResponse, C.ResultSet);
            Row = resultSet.ResultSetRow;
        }
    }
}