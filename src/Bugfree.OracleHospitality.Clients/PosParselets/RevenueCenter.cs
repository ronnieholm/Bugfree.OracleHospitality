using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class RevenueCenter : IRequestElement
{
    public const int MinValue = 0;
    public const int MaxValue = 999_999_999;

    public int Value { get; }

    public RevenueCenter(string value)
    {
        Value = FieldTypes.AssertN9(value);
    }

    public RevenueCenter(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentException($"{nameof(value)} must in range [{MinValue};{MaxValue}]. Was {value}");
        Value = value;
    }

    public override string ToString() => Value.ToString();
}