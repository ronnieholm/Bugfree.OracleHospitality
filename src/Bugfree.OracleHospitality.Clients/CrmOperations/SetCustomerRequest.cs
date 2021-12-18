using System;
using System.Linq;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.CrmOperations;

public class SetCustomerRequest : CrmRequest
{
    public int? RowId { get; }
    public ColumnValue[] ColumnValues { get; }

    public SetCustomerRequest(string requestSourceName, int? rowId, ColumnValue[] columnValues)
        : base(requestSourceName)
    {
        if (rowId.HasValue && rowId.Value <= 0)
            throw new ArgumentException($"{nameof(rowId)} must be positive. Was {rowId}");
        if (columnValues == null)
            throw new ArgumentNullException(nameof(columnValues));
        if (columnValues.Length == 0)
            throw new ArgumentException($"{nameof(columnValues)} must not be empty");

        RowId = rowId;
        ColumnValues = columnValues;
    }

    public override XE BuildRequestDocument()
    {
        var request = BuildBaseDocument(RequestCode.Kind.SetCustomer);
        request.Add(
            new XE(C.DataSet,
                new XE(C.DataSetColumns,
                    ColumnValues.Select(kvp => new XE(C.DSColumn, new XA(C.name, kvp.Column)))),
                new XE(C.Rows,
                    new XE(C.Row, RowId == null ? null : new XA(C.id, RowId),
                        ColumnValues.Select(kvp => new XE(C.Col, kvp.Value))))));
        return request;
    }
}