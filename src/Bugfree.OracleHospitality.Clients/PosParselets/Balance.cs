using System.Globalization;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class Balance : IResponseElement
    {
        public decimal Value { get; }
        public Balance(string value) => Value = FieldTypes.AssertDecimal(value);
        public Balance(decimal value) => Value = value;
        public override string ToString() => Value.ToString(CultureInfo.GetCultureInfo("en-US"));
    }
}