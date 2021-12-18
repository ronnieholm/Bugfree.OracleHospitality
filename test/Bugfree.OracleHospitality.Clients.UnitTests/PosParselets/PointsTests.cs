using System;
using Xunit;
using System.Globalization;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class PointsTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("10")]
    [InlineData("100")]
    [InlineData("1.0")]
    [InlineData("1.23")]
    [InlineData("1234567890.0987654321")]
    public void valid(string points)
    {
        var a = new Points(points);
        Assert.Equal(decimal.Parse(points, CultureInfo.GetCultureInfo("en-US")), a.Value);
        Assert.Equal(points, a.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("-1")]
    [InlineData("-")]
    [InlineData("+1")]
    [InlineData("0.")]
    [InlineData("01")]
    [InlineData("1,25")]
    public void invalid(string points)
    {
        var _ = Assert.Throws<ArgumentException>(() => new Points(points));
    }
}