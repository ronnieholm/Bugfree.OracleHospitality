using System;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    // Represents multiple elements: GetCoupons and GetCustomer uses it for
    // their Condition node; SetCustomer uses it for DSColumn (column) and Col
    // (values) nodes.
    public class ColumnValue : IRequestElement
    {
        public Column Column { get; }
        public string Value { get; }

        public ColumnValue(string columnName, string columnValue)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException(nameof(columnName));
            if (string.IsNullOrWhiteSpace(columnValue))
                throw new ArgumentException(nameof(columnValue));

            Column = new Column(columnName);
            Value = columnValue;
        }
    }
}