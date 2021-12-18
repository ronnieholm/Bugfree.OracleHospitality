using System;
using Bugfree.OracleHospitality.Clients.PosParselets;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.PosOperations;

public class CouponInquiryRequest : PosRequest
{
    public CouponInquiryRequest(DateTime timestamp, MessageId messageId, AccountNumber accountNumber)
        : base(timestamp, messageId, accountNumber)
    {
    }

    public override XE BuildRequestDocument() =>
        BuildBaseDocument(new RequestCode(TransactionKind.COUPON_INQUIRY));
}