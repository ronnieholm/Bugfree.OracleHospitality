namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class DisplayMessage : IResponseElement
    {
        public string Value { get; }
        public DisplayMessage(string value) => Value = FieldTypes.AssertString(value);
        public override string ToString() => Value;
    }
}