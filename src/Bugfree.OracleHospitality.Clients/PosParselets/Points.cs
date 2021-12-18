using System;
using System.Globalization;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class Points : IResponseElement
{
    public decimal Value { get; }

    public Points(string value)
    {
        Value = FieldTypes.AssertDecimal(value);
        if (Value < 0)
            throw new ArgumentException($"{nameof(Points)} must be positive. Was {value}");
    }

    public override string ToString() => Value.ToString(CultureInfo.GetCultureInfo("en-US"));
}