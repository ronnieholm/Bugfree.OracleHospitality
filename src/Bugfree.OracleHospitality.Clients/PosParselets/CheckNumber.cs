using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class CheckNumber : IRequestElement
    {
        public const int MinValue = 0;
        public const int MaxValue = 9999;

        public static CheckNumber Reset => new CheckNumber { Value = 0 };
        public int Value { get; private set; }

        private CheckNumber() { }

        public CheckNumber(string value)
        {
            // UNDOCUMENTED: CheckNumber element is missing from POS API spec,
            // Page 19, Element Definitions. We assume CheckNumber is identical
            // to the CheckNumber part of TraceID which is N4.
            Value = FieldTypes.AssertN4(value);
        }

        public CheckNumber(int value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentException($"Expected {nameof(value)} to be in range [{MinValue}, {MaxValue}]. Was {value}");
            Value = value;
        }

        public CheckNumber Increment()
        {
            // Wrap around would happen after 9999 retries, but we disallow wrap
            // around. Unlike Sequence number which must only be incremented
            // upon successful request/response, CheckNumber must be incremented
            // with every request.
            //
            // UNDOCUMENTED: the purpose of CheckNumber is unclear. Because
            // issuing a new retry request, its timestamp would be updated,
            // making TraceID unique withou CheckNumber.
            if (Value >= MaxValue)
                throw new ArgumentException($"{nameof(CheckNumber)} overflow");
            return new CheckNumber(Value + 1);
        }

        public override string ToString() => Value.ToString("D4");
    }
}