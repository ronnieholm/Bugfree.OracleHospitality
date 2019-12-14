using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    public class GetProgramResponse : CrmResponse
    {
        public ResultSetMetaDataColumn[] MetaData { get; private set; }
        public Row[] Rows { get; private set; }

        public GetProgramResponse(XE request, XE response)
            : base(request, response)
        {
        }

        protected override void DeconstructResponse()
        {
            if (ExceptionToRaiseAfterParsing != null)
                return;

            var resultSetElement = ExpectElement(Response_, C.ResultSet);
            var resultSet = new ResultSet(resultSetElement);
            ConsumeElement(UnconsumedResponse, C.ResultSet);
            MetaData = resultSet.ResultSetMetaDataColumns;
            Rows = resultSet.ResultSetRows;
        }
    }
}