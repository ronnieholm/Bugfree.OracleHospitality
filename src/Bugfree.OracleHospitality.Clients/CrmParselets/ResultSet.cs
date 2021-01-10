using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using XE = System.Xml.Linq.XElement;
using K = Bugfree.OracleHospitality.Clients.CrmParselets.ResultSetMetaDataColumn.Type.Kind;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class ResultSetMetaDataColumn : IResponseElement
    {
        public class Name : IResponseAttribute
        {
            public string Value { get; }
            public Name(string value) => Value = FieldTypes.AssertString(value);
        }

        public class Type : IResponseAttribute
        {
            // UNDOCUMENTED: mapping between Kind and Ax, Nx, ... field types is
            // inferred/generated/validated by the end_to_end integration test.
            // For instance, string actually means something along the lines of
            // A1,A10,A150,A16,A200,A25,A3,A32,A4,A50,A76. The end_to_end
            // integration test ensures that the nX types fit within the CLR
            // types used to represent these.
            public enum Kind
            {
                None,
                String,
                Timestamp,
                Boolean,
                Long,
                Short,
                Double,
                Int
            }

            public K Value { get; }

            public Type(string value)
            {
                FieldTypes.AssertString(value);
                Value = value switch
                {
                    "string" => K.String,
                    "timestamp" => K.Timestamp,
                    "boolean" => K.Boolean,
                    "long" => K.Long,
                    "short" => K.Short,
                    "double" => K.Double,
                    "int" => K.Int,
                    _ => throw new ArgumentException($"Unsupported kind: '{value}"),
                };
            }
        }

        public class Nullable : IResponseAttribute
        {
            public bool Value { get; }
            public Nullable(string value) => Value = FieldTypes.AssertBoolean(value);
        }

        public Name Name_ { get; }
        public Type Type_ { get; }
        public Nullable Nullable_ { get; }

        public ResultSetMetaDataColumn(XE resultSetColumn)
        {
            var nameAttribute = ExpectAttribute(resultSetColumn, C.name);
            Name_ = new Name(nameAttribute.Value);
            var typeAttribute = ExpectAttribute(resultSetColumn, C.type);
            Type_ = new Type(typeAttribute.Value);
            var nullableAttribute = ExpectAttribute(resultSetColumn, C.nullable);
            Nullable_ = new Nullable(nullableAttribute.Value);
        }
    }

    public class ResultSetMetaData : IResponseElement
    {
        public ResultSetMetaDataColumn[] Columns { get; set; }

        public ResultSetMetaData(XE resultSetMetaData)
        {
            var resultSetColumns = ExpectElements(resultSetMetaData, C.RSColumn);
            Columns = resultSetColumns.Select(c => new ResultSetMetaDataColumn(c)).ToArray();
        }
    }

    // Parses one Row element in the format below. Multiple customers may be 
    // associated with an account, but typically only one is. Row id is 
    // optionally present in the responses:
    //
    // <Rows>
    //   <Row id="416440"">
    //     <Col>Doe</Col>
    //     <Col>John</Col>
    //   </Row>
    //   <Row id=""422056">
    //     <Col />
    //     <Col />
    //   </Row>
    //   ...
    // </Rows>
    public class Row : IResponseElement
    {
        public int? Id { get; }
        public Dictionary<string, object> Columns { get; }

        private static object Map(string value, K type)
        {
            return type switch
            {
                K.String => string.IsNullOrEmpty(value) ? "" : value,
                K.Boolean => value != "0" && value != "false",
                K.Timestamp => string.IsNullOrEmpty(value) ? default : DateTime.Parse(value),
                K.Long => string.IsNullOrEmpty(value) ? default : long.Parse(value),
                K.Short => string.IsNullOrEmpty(value) ? default : short.Parse(value),
                K.Double => string.IsNullOrEmpty(value) ? default : decimal.Parse(value, CultureInfo.GetCultureInfo("en-US")),
                K.Int => string.IsNullOrEmpty(value) ? default : int.Parse(value),
                _ => throw new ArgumentException($"Unsupported type: {type}")
            };
        }

        public Row(ResultSetMetaDataColumn[] metaData, XE rowElement)
        {
            // Rows in operations such as GetCouponResultSet include no row id
            // whereas GetCustomerResultSet does.
            if (rowElement.Attribute(C.id) == null)
                Id = null;
            else
            {
                var idAttribute = ExpectAttribute(rowElement, C.id);
                Id = FieldTypes.AssertInteger(idAttribute.Value);
            }

            var columns = rowElement.Elements(C.Col).ToArray();
            Columns = new Dictionary<string, object>();

            for (var i = 0; i < columns.Length; i++)
            {
                var name = metaData[i].Name_.Value;
                var type = metaData[i].Type_.Value;
                var nullable = metaData[i].Nullable_.Value;
                var empty = columns[i].IsEmpty;
                var value = columns[i].Value;

                object mappedValue = (empty, nullable) switch
                {
                    (true, true) => null,
                    (false, true) => Map(value, type),
                    (false, false) => Map(value, type),
                    (true, false) => Map(value, type),
                };
                Columns.Add(name, mappedValue);
            }
        }
    }

    public class ResultSet : IResponseElement
    {
        public ResultSetMetaDataColumn[] ResultSetMetaDataColumns { get; set; }
        public Row[] ResultSetRows { get; set; }

        public ResultSet(XE resultSet)
        {
            var resultSetMetaDataElement = ExpectElement(resultSet, C.ResultSetMetaData);
            var metadata = new ResultSetMetaData(resultSetMetaDataElement);
            ResultSetMetaDataColumns = metadata.Columns;

            var rowsElement = ExpectElement(resultSet, C.Rows);
            var rowElements = rowsElement.Elements(C.Row);
            ResultSetRows = rowElements.Select(r => new Row(ResultSetMetaDataColumns, r)).ToArray();
        }
    }

    public class GetColumnListResultSet : IResponseElement
    {
        // Parses a Row element of the form
        //
        // <Rows>
        //   <Row>
        //     <Col type="A10">PREFIX</Col>
        //     <Col type="N19">PARENTCACUSTOMERID</Col>
        //     <Col type="A25">MOBILEPHONENUMBER</Col>
        //     ...
        //   <Row>
        // <Rows>
        public class Row : IResponseElement
        {
            public Dictionary</* Column content */ string, /* type */ string> Columns { get; }

            public Row(XE rowElement)
            {
                var columns = rowElement.Elements(C.Col).ToArray();
                Columns = new Dictionary<string, string>();

                foreach (var c in columns)
                {
                    var typeAttribute = ExpectAttribute(c, C.type);
                    var key = FieldTypes.AssertString(c.Value);
                    var value = FieldTypes.AssertString(typeAttribute.Value);
                    Columns.Add(key, value);
                }
            }
        }

        public Row ResultSetRow { get; set; }

        public GetColumnListResultSet(XE resultSet)
        {
            var rowsElement = ExpectElement(resultSet, C.Rows);
            var rowElement = ExpectElement(rowsElement, C.Row);
            ResultSetRow = new Row(rowElement);
        }
    }
}