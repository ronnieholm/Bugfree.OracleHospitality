namespace Bugfree.OracleHospitality.Clients.CrmParselets;

// UNDOCUMENTED: this attribute and its purpose is not described and is
// included in samples only.
public class IsTrustedSat : IResponseAttribute
{
    public bool Value { get; }
    public IsTrustedSat(string value) => Value = FieldTypes.AssertBoolean(value);
    public override string ToString() => Value ? "true" : "false";
}