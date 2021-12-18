using System;
using XE = System.Xml.Linq.XElement;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.PosOperations;

public class PointIssueRequest : PosRequest
{
    public PointIssueRequest(DateTime timestamp, MessageId messageId, AccountNumber accountNumber)
        : base(timestamp, messageId, accountNumber)
    {
    }

    public override XE BuildRequestDocument() =>
        BuildBaseDocument(new RequestCode(TransactionKind.POINT_ISSUE));
}