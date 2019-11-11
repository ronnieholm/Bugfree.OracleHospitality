namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class HostVersion : IResponseAttribute
    {
        public string Value { get; }
        public HostVersion(string value) =>  Value = FieldTypes.AssertString(value);
        public override string ToString() => Value;
    }
}
