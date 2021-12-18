using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class LocalDateTests
{
    [Theory]
    [InlineData("20190724")]
    public void valid(string date)
    {
        var ld = new LocalDate(date);
        Assert.Equal(new DateTime(2019, 7, 24), ld.Value);
        Assert.Equal(date, ld.ToString());
    }

    [Theory]
    [InlineData("")] // Too short
    [InlineData(" ")] // Whitespace
    [InlineData("2019072")] // Too short
    [InlineData("201907244")] // Too long
    [InlineData("20191324")] // Invalid month
    [InlineData("20191232")] // Invalid day
    public void invalid(string date)
    {
        var _ = Assert.Throws<ArgumentException>(() => new LocalDate(date));
    }
}