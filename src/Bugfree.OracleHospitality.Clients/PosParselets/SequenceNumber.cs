using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class SequenceNumber : IRequestAttribute, IResponseAttribute
{
    public const int MinValue = 0;
    public const int MaxValue = 99;

    public static SequenceNumber Reset => new SequenceNumber { Value = 0 };
    public int Value { get; private set; }

    private SequenceNumber() { }
    public SequenceNumber(string value) => Value = FieldTypes.AssertN2(value);

    public SequenceNumber(int value)
    {
        if (value is < MinValue or > MaxValue)
            throw new ArgumentException($"Expected {nameof(value)} to be in range {MinValue} to {MaxValue}. Was {value}");
        Value = value;
    }

    public SequenceNumber Increment()
    {
        // Zero is used to indicate terminal power reset to the backend. In
        // any other case, when sequence number reaches 100, it must wrap
        // around to one. 
        //
        // UNDOCUMENTED: from POS API spec, Page 14, it's unclear if and
        // what the Oracle backend uses this information for.
        if (Value >= MaxValue)
            return new SequenceNumber(1);
        return new SequenceNumber(Value + 1);
    }

    public override string ToString() => Value.ToString("D2");
}