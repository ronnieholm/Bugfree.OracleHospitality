using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class ColumnTest
{
    [Theory]
    [InlineData("firstname")]
    public void valid(string columnName)
    {
        var c = new Column(columnName);
        Assert.Equal(columnName, c.Name);
        Assert.Equal(columnName, c.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void invalid(string columnName)
    {
        var e = Assert.Throws<ArgumentException>(() => new Column(columnName));
        Assert.Contains("name", e.Message);
    }
}