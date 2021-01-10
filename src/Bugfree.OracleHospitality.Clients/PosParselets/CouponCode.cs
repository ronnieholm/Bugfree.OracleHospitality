using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    // UNDOCUMENTED: element isn't included in list of elements, but shows up
    // with the Accept Coupon transaction.
    //
    // Meaning of CouponCode element is ambigous: with ISSUE_COUPON request
    // it's <CouponCode>10DKK</CouponCode> but in its response it's the issued
    // coupon's serial number: <CouponCode>1006022</CouponCode>.
    public class CouponCode : IRequestElement
    {
        public string Value { get; }

        public CouponCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{nameof(value)} must not be null or whitespace");
            Value = value;
        }

        public override string ToString() => Value;
    }
}