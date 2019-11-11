using System.Globalization;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    // UNDOCUMENTED: element is unspecified, but given that other amount-like
    // elements are FieldType.Decimal, we assume it's the case for Amount, too.
    public class Amount : IRequestElement, IResponseAttribute
    {
        // Because of an unlimited number of digits, according to spec, we
        // cannot store Amount as .NET decimal without possible loss of
        // precision. However, we assume decimal is good enough. If it isn't,
        // during asserting throws an exception.
        public decimal Value { get; }
        public Amount(string value) => Value = FieldTypes.AssertDecimal(value);
        public Amount(decimal value) => Value = value;
        public override string ToString() => Value.ToString("0.00", CultureInfo.GetCultureInfo("en-US"));
    }
}