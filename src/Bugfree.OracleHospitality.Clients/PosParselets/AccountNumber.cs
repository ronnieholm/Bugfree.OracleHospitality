namespace Bugfree.OracleHospitality.Clients.PosParselets;

// UNDOCUMENTED: while POS API spec specifies account number as A32 that
// isn't true. When associating an account to a program, account number
// becomes A24 of which at least one character is reserved for preamble.
public class AccountNumber : IRequestElement, IResponseElement
{
    public string Value { get; }
    public AccountNumber(string value) => Value = FieldTypes.AssertA32(value);
    public override string ToString() => Value;
}