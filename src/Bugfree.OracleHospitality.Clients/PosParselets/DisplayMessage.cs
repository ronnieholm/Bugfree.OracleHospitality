namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class DisplayMessage : IResponseElement
{
    public string Value { get; }
    public DisplayMessage(string value) => Value = FieldTypes.AssertA100(value);
    public override string ToString() => Value;
}