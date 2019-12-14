using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    // As an exception to sticking to Oracle terminology across the client
    // implementations, we rename Oracle's Retransmit attribute to Transmission
    // because it's a property of something (noun) and not an action (verb).
    // It's not a retransmission because most of the time the attribute assumes
    // the value of Normal.
    //
    // UNDOCUMENTED: according to POS API spec, Page 13 should be "y",
    // indicating a retransmit, if the original request failed due to a timeout.
    // Documentation doesn't state if that's the only condition requiring a
    // 'y' or how to determine when timeout was the cause.
    public class Transmission : IRequestAttribute, IResponseAttribute
    {
        public enum Kind
        {
            None,
            Normal,
            Retransmission
        }

        public Kind Value { get; }

        public Transmission(string value)
        {
            FieldTypes.AssertA1(value);
            Value = value switch
            {
                "n" => Kind.Normal,
                "y" => Kind.Retransmission,
                _ => throw new ArgumentException($"Unsupported {nameof(Kind)} value: '{value}'")
            };
        }

        public Transmission(Kind kind)
        {
            if (kind == Kind.None)
                throw new ArgumentException($"{nameof(Kind)} must not be {kind}");
            Value = kind;
        }

        public override string ToString()
        {
            return Value switch
            {
                Kind.Normal => "n",
                Kind.Retransmission => "y",
                _ => throw new ArgumentException($"Unsupported {nameof(Kind)}: {Value}"),
            };
        }
    }
}