using System;
using System.Globalization;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class ExchangeRate : IResponseElement
    {
        public decimal Value { get; }

        public ExchangeRate(string value)
        {
            Value = FieldTypes.AssertDecimal(value);
            if (Value <= 0)
                throw new ArgumentException($"{nameof(ExchangeRate)} rate must be positive. Was {value}");
        }

        public override string ToString() => Value.ToString(CultureInfo.GetCultureInfo("en-US"));
    }
}