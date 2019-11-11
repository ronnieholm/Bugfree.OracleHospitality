using System;
using System.Linq;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;

// DESIGN: we could put these helpers in a base class shared across CRM and POS
// APIs requests/responses classes. That would introduce another level in the
// inheritance hierarchy with <some-crm-response> inheriting from CrmResponse
// inheriting from Response where methods below would go. Such deep inheritance
// hierarchy couples components unnecessarity.

namespace Bugfree.OracleHospitality.Clients
{
    public static class ParserHelpers
    {
        public static XA ExpectAttribute(XE element, string attributeName)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (attributeName == null)
                throw new ArgumentNullException(nameof(attributeName));
            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentException($"{nameof(attributeName)} must not be null or whitespace. Was $'{attributeName}'");

            var a = element.Attribute(attributeName);
            if (a == null)
                throw new ArgumentException($"Expected '{attributeName}' attribute");
            return a;
        }

        public static XE ExpectElement(XE element, string elementName)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (elementName == null)
                throw new ArgumentNullException(nameof(elementName));
            if (string.IsNullOrWhiteSpace(elementName))
                throw new ArgumentException($"{nameof(elementName)} must not be null or whitespace. Was $'{elementName}'");

            var e = element.Element(elementName);
            if (e == null)
                throw new ArgumentException($"Expected '{elementName}' element");
            return e;
        }

        public static XE[] ExpectElements(XE element, string elementName)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (elementName == null)
                throw new ArgumentNullException(nameof(elementName));
            if (string.IsNullOrWhiteSpace(elementName))
                throw new ArgumentException($"{nameof(elementName)} must not be null or whitespace. Was $'{elementName}'");

            var es = element.Elements(elementName).ToArray();
            if (es.Length == 0)
                throw new ArgumentException($"Expected at least one '{elementName}' element");
            return es;
        }

        public static void ConsumeAttribute(XE unconsumed, string attributeName)
        {
            var attribute = ExpectAttribute(unconsumed, attributeName);
            attribute.Remove();
        }

        public static void ConsumeElement(XE unconsumed, string elementName)
        {
            var element = ExpectElement(unconsumed, elementName);
            element.Remove();
        }

        public static void MapAttribute(XE response, string attributeName, Action<XA> creatorFn)
        {
            if (creatorFn == null)
                throw new ArgumentNullException(nameof(creatorFn));
            var attribute = ExpectAttribute(response, attributeName);
            creatorFn(attribute);
        }

        public static void MapElement(XE response, string elementName, Action<XE> creatorFn)
        {
            if (creatorFn == null)
                throw new ArgumentNullException(nameof(creatorFn));
            var element = ExpectElement(response, elementName);
            creatorFn(element);
        }

        public static void ValidateAttribute(XE request, XE response, string attributeName, Func<string, string, bool> comparatorFn)
        {
            if (comparatorFn == null)
                throw new ArgumentNullException(nameof(comparatorFn));
            var a = ExpectAttribute(request, attributeName);
            var b = ExpectAttribute(response, attributeName);
            if (!comparatorFn(a.Value, b.Value))
                throw new OracleHospitalityClientException($"Expected attribute values for '{attributeName}' to be equal. Was '{a.Value}' and '{b.Value}'");
        }

        public static void ValidateElement(XE request, XE response, string elementName, Func<string, string, bool> comparatorFn)
        {
            if (comparatorFn == null)
                throw new ArgumentNullException(nameof(comparatorFn));
            var a = ExpectElement(request, elementName);
            var b = ExpectElement(response, elementName);
            if (!comparatorFn(a.Value, b.Value))
                throw new OracleHospitalityClientException($"Expected element values for '{elementName}' to be equal. Was '{a.Value}' and '{b.Value}'");
        }
    }
}