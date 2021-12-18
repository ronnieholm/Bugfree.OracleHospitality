using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

public class HostVersion : IResponseAttribute
{
    public string Value { get; }

    public HostVersion(string value)
    {
        // UNDOCUMENTED: POS API spec, Page 18, designates hostVersion
        // attribute as A8. "1.00.8" example in documentation supports this
        // field type, but time seems to have surpassed documentation.
        // Current hostVersion is "9.1.0000.2301" which doesn't fit in A8.
        // We assume field is A16.
        Value = FieldTypes.AssertA16(value);

        // UNDOCUMENTED: Oracle's versioning strategy is unclear. Which
        // component of the version number, if any, would change to signal a
        // breaking change? For now, we hardcode version to force an
        // explicit failure on Oracle update.
        const string hostVersion = "9.1.0000.2301";
        if (value != hostVersion)
            throw new ArgumentException($"Oracle updated POS host version from '{hostVersion}' to '{value}'");
    }

    public override string ToString() => Value;
}