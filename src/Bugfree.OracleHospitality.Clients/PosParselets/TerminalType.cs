namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class TerminalType : IRequestElement
{
    public string Value { get; }
    public TerminalType(string value) => Value = FieldTypes.AssertA8(value);
    public override string ToString() => Value;
}