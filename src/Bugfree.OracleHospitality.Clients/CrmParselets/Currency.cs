using System;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class Currency : IRequestAttribute
    {
        public enum Kind
        {
            None,
            USD,
            DKK
        }

        public Kind Value { get; }

        public Currency(string value)
        {
            FieldTypes.AssertString(value);
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