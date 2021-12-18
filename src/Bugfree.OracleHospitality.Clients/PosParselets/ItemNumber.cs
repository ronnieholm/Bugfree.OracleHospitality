namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class ItemNumber : IResponseElement
{
    public int Value { get; }
    public ItemNumber(string value) => Value = FieldTypes.AssertN9(value);
    public override string ToString() => Value.ToString();
}