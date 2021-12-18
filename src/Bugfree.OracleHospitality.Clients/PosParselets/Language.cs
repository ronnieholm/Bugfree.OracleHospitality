using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class Language : IRequestAttribute
{
    // We use a class over an enum because "-" would be an illegal enum
    // value.
    public static class Kind
    {
        public const string EnUs = "en-US";
    }

    public string Value { get; }

    public Language(string value)
    {
        FieldTypes.AssertA5(value);

        if (value != Kind.EnUs)
            throw new ArgumentException($"Expected '{Kind.EnUs}'. Was {value}");

        Value = value;
    }

    public override string ToString() => Value;
}