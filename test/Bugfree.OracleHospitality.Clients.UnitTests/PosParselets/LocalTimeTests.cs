using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class LocalTimeTests
{
    [Theory]
    [InlineData("133254")]
    public void valid(string time)
    {
        var lt = new LocalTime(time);
        Assert.Equal(new TimeSpan(13, 32, 54), lt.Value);
        Assert.Equal(time, lt.ToString());
    }

    [Theory]
    [InlineData("")] // Too short
    [InlineData(" ")] // Whitespace
    [InlineData("13325")] // Too short
    [InlineData("1332541")] // Too long
    [InlineData("253254")] // Invalid hour
    [InlineData("256054")] // Invalid minute
    [InlineData("250060")] // Invalid second
    public void invalid(string time)
    {
        var _ = Assert.Throws<ArgumentException>(() => new LocalTime(time));
    }
}