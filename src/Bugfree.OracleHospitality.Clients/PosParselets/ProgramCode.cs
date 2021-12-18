namespace Bugfree.OracleHospitality.Clients.PosParselets;

// UNDOCUMENTED: POS API spec incorrectly specifies it as A8. Based on
// observations in the wild it's at least A16.
public class ProgramCode : IResponseElement
{
    public string Value { get; }
    public ProgramCode(string value) => Value = FieldTypes.AssertA16(value);
    public override string ToString() => Value;
}