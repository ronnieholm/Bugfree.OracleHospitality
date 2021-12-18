using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

// From POS API spec, Page 15, SVC Transaction Message List
public enum TransactionKind
{
    None,
    COUPON_INQUIRY,
    SV_ISSUE_COUPON,

    // Erroneously named SVC_ACCEPT_COUPON in POS API spec, Page 16
    SV_ACCEPT_COUPON,
    POINT_ISSUE
}

public class RequestCode : IRequestElement, IResponseElement
{
    public TransactionKind Value { get; }

    public RequestCode(string value)
    {
        FieldTypes.AssertA100(value);
        if (!Enum.IsDefined(typeof(TransactionKind), value))
            throw new ArgumentException($"Unknown value '{value}'");
        Value = (TransactionKind)Enum.Parse(typeof(TransactionKind), value);
    }

    public RequestCode(TransactionKind kind)
    {
        if (kind == TransactionKind.None)
            throw new ArgumentException($"{nameof(TransactionKind)} must not be {kind}");
        Value = kind;
    }

    public override string ToString() => Value.ToString();
}