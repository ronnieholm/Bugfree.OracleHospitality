using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class ColumnValueTests
{
    [Theory]
    [InlineData("firstname", "Rubber")]
    public void valid(string columnName, string columnValue)
    {
        var cv = new ColumnValue(columnName, columnValue);
        Assert.Equal(columnName, cv.Column.Name);
        Assert.Equal(columnValue, cv.Value);
    }

    [Theory]
    [InlineData("firstname", "")]
    [InlineData("", "Rubber")]
    [InlineData(null, "")]
    [InlineData(null, "Duck")]
    public void invalid(string columnName, string columnValue)
    {
        var e = Assert.Throws<ArgumentException>(() => new ColumnValue(columnName, columnValue));
        Assert.True(
            e.Message.Contains("columnName") || e.Message.Contains("columnValue"),
            e.Message);
    }
}