using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmParselets;

public class RequestSource : IRequestElement
{
    public class Version : IRequestAttribute
    {
        public string Value { get; }
        public Version(string value) => Value = FieldTypes.AssertString(value);
        public override string ToString() => Value;
    }

    public class Name : IRequestAttribute
    {
        public string Value { get; }
        public Name(string value) => Value = FieldTypes.AssertString(value);
        public override string ToString() => Value;
    }

    public Name Name_ { get; }

    public RequestSource(XE requestSource)
    {
        var nameAttribute = ExpectAttribute(requestSource, C.name);
        Name_ = new Name(nameAttribute.Value);
    }
}