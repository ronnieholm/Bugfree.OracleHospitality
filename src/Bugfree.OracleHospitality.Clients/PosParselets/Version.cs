namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class Version : IRequestAttribute, IResponseAttribute
{
    public string Value { get; }
    public Version(string value) => Value = FieldTypes.AssertA8(value);
    public override string ToString() => Value;
}