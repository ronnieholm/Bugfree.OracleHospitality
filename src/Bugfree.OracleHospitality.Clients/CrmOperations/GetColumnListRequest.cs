using System;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.CrmOperations;

public class GetColumnListRequest : CrmRequest
{
    public string Entity { get; }

    public GetColumnListRequest(string requestSourceName, string entity)
        : base(requestSourceName)
    {
        if (string.IsNullOrWhiteSpace(entity))
            throw new ArgumentException(nameof(entity));
        Entity = entity;
    }

    public override XE BuildRequestDocument()
    {
        var request = BuildBaseDocument(RequestCode.Kind.GetColumnList);
        request.Add(new XE(C.QueryCriteria, new XA(C.request, Entity)));
        return request;
    }
}