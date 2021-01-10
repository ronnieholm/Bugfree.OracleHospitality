namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    // Only attributes of top-level CRMMessage elements get their own file in
    // this folder. Each class represents one XML element, and if the element
    // has attributes or child elements, those become inner classes to prevent
    // name collisions, e.g., name is an attribute of both RequestSource and
    // Condition elements, but serve different purposes and is thus represented
    // by different types.

    public class Language : IRequestAttribute
    {
        public static class Kind
        {
            // CRM API uses underscore whereas POS API uses dash.
            public const string EnUs = "en_US";
        }

        public string Value { get; }
        public Language(string value) => Value = FieldTypes.AssertString(value);
        public override string ToString() => Value;
    }
}