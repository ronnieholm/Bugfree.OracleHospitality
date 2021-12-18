using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class BusinessDateTests
{
    [Theory]
    [InlineData("20190724")]
    public void valid(string date)
    {
        var bd = new BusinessDate(date);
        Assert.Equal(new DateTime(2019, 7, 24), bd.Value);
        Assert.Equal(date, bd.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("2019072")] // Too short
    [InlineData("201907244")] // Too long        
    [InlineData("20191324")] // Invalid month
    [InlineData("20191232")] // Invalid day
    public void invalid(string date)
    {
        var _ = Assert.Throws<ArgumentException>(() => new BusinessDate(date));
    }
}