namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class PosInterfaceVersion : IRequestAttribute, IResponseAttribute
{
    public string Value { get; }
    public PosInterfaceVersion(string value) => Value = FieldTypes.AssertA8(value);
    public override string ToString() => Value;
}