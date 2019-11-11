using System;
using System.Linq;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    public class GetAccountRequest : CrmRequest
    {
        public string Conditions { get; }
        public ColumnValue[] ColumnValues { get; }
        public Column[] Columns { get; }

        public GetAccountRequest(string requestSourceName, string conditions, ColumnValue[] columnValues, Column[] columns)
            : base(requestSourceName)
        {
            if (string.IsNullOrWhiteSpace(conditions))
                throw new ArgumentException(nameof(conditions));
            if (columnValues == null)
                throw new ArgumentNullException(nameof(columnValues));
            if (columnValues.Length == 0)
                throw new ArgumentException($"{nameof(columnValues)} must not be empty");
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (columns.Length == 0)
                throw new ArgumentException($"{nameof(columns)} must not be empty");

            Conditions = conditions;
            ColumnValues = columnValues;
            Columns = columns;
        }

        public override XE BuildRequestDocument()
        {
            var request = BuildBaseDocument(RequestCode.Kind.GetAccount);
            request.Add(new XE(C.QueryCriteria, new XA(C.conditions, Conditions),
                ColumnValues.Select(condition =>
                    new XE(C.Condition, new XA(C.name, condition.Column), new XA(C.value, condition.Value)))));
            request.Add(new XE(C.ResultSetDefinition,
                Columns.Select(column =>
                    new XE(C.Column, column.Name))));
            return request;
        }
    }
}