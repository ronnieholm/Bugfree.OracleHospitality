using System;
using Bugfree.OracleHospitality.Clients.PosParselets;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.PosOperations;

public class CouponAcceptRequest : PosRequest
{
    private readonly CouponCode _couponCode;

    public CouponAcceptRequest(DateTime timestamp, MessageId messageId, AccountNumber accountNumber, CouponCode couponCode)
        : base(timestamp, messageId, accountNumber)
    {
        _couponCode = couponCode;
    }

    public override XE BuildRequestDocument()
    {
        var request = BuildBaseDocument(new RequestCode(TransactionKind.SV_ACCEPT_COUPON));
        request.Add(new XE(C.CouponCode, _couponCode));
        return request;
    }
}