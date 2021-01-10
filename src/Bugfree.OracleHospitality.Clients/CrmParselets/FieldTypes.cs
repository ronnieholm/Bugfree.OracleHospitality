using System;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    static class FieldTypes
    {
        public static string AssertString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Must not be null or whitespace");
            return s;
        }

        public static bool AssertBoolean(string s)
        {
            AssertString(s);
            if (!(s == "true" || s == "false"))
                throw new ArgumentException($"Only 'true' or 'false' supported. Got '{s}'");

            // bool.Parse successfully parses "True" and "False" which API
            // doesn't return. Thus we first limit options.
            return bool.Parse(s);
        }

        public static DateTime AssertTimestamp(string s)
        {
            AssertString(s);
            return DateTime.Parse(s);
        }

        public static int AssertInteger(string s)
        {
            var success = int.TryParse(s, out var integer);
            if (!success)
                throw new ArgumentException($"Cannot convert to System.Int32: {s}");
            return integer;
        }

        public static decimal AssertDecimal(string s)
        {
            var success = decimal.TryParse(s, out var @decimal);
            if (!success)
                throw new ArgumentException($"Cannot convert to System.Decimal: {s}");
            return @decimal;
        }
    }
}