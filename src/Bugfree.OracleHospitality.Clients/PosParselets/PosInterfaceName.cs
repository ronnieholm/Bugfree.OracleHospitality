namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class PosInterfaceName : IRequestAttribute, IResponseAttribute
{
    public string Value { get; }
    public PosInterfaceName(string value) => Value = FieldTypes.AssertA16(value);
    public override string ToString() => Value;
}