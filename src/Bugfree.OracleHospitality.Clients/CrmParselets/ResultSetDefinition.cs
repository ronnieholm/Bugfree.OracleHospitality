using System;
using System.Collections.ObjectModel;
using System.Linq;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    // Parses:
    //
    // <ResultSetDefinition>
    //   <Column>firstName</Column>
    //   <Column>LastName</Column>
    // </ResultSetDefinition>
    public class ResultSetDefinition : IRequestElement
    {
        public class Column : IRequestElement
        {
            public string Value { get; }
            public Column(string value) => Value = FieldTypes.AssertString(value);
            public override string ToString() => Value;
        }

        public ReadOnlyCollection<Column> Columns { get; }

        public ResultSetDefinition(XE resultSetDefinition)
        {
            var columnElement = ExpectElements(resultSetDefinition, C.Column);
            Columns = Array.AsReadOnly(columnElement.Select(ce => new Column(ce.Value)).ToArray());
        }
    }
}