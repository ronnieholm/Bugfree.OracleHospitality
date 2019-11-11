namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class Balance
    {
        public decimal Value { get; }
        public Balance(string value) => Value = FieldTypes.AssertDecimal(value);
    }
}
