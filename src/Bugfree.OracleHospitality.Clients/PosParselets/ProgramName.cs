namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class ProgramName : IResponseElement
{
    public string Value { get; }
    public ProgramName(string value) => Value = FieldTypes.AssertA32(value);
    public override string ToString() => Value;
}