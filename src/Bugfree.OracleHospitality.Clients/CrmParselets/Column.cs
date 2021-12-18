using System;

namespace Bugfree.OracleHospitality.Clients.CrmParselets;

public class Column : IRequestElement
{
    public string Name { get; }

    public Column(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));
        Name = name;
    }

    public override string ToString() => Name;
}