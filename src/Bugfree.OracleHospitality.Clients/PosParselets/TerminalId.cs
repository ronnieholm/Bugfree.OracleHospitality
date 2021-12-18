using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class TerminalId : IRequestElement
{
    public const int MinValue = 0;
    public const int MaxValue = 999_999_999;

    public int Value { get; }

    public TerminalId(string value)
    {
        FieldTypes.AssertN9(value);
        Value = int.Parse(value);
    }

    public TerminalId(int value)
    {
        if (value is < MinValue or > MaxValue)
            throw new ArgumentException($"Expected {nameof(value)} to be in range [{MinValue};{MaxValue}]. Was {value}");
        Value = value;
    }

    public override string ToString() => Value.ToString();
}