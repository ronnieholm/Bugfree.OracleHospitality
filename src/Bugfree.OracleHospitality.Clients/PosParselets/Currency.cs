using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    // Used for both currency attribute and LocalCurrency element
    public class Currency : IRequestAttribute, IRequestElement, IResponseElement
    {
        // ISO 4217 standard formats as
        // perhttps://en.wikipedia.org/wiki/ISO_4217.
        public enum Kind
        {
            None,
            DKK
        }

        public Kind Value { get; }

        public Currency(string value)
        {
            FieldTypes.AssertA3(value);

            // Not only must value not be null or whitespace and satisfy A3,
            // only Kind is allowed.
            if (!Enum.IsDefined(typeof(Kind), value))
                throw new ArgumentException($"Error parsing {nameof(Kind)}: {value}");
            Value = (Kind)Enum.Parse(typeof(Kind), value);
        }

        public Currency(Kind kind)
        {
            if (kind == Kind.None)
                throw new ArgumentException($"{nameof(Kind)} must not be {kind}");
            Value = kind;
        }

        public override string ToString() => Value.ToString();
    }
}