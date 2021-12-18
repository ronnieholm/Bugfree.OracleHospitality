using System;
using Xunit;
using System.Globalization;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class AmountTests
{
    [Theory]
    [InlineData("0", "0.00")]
    [InlineData("10", "10.00")]
    [InlineData("100", "100.00")]
    [InlineData("-1", "-1.00")]
    [InlineData("1.0", "1.00")]
    [InlineData("1.23", "1.23")]
    [InlineData("1234567890.0987654321", "1234567890.10")]
    public void valid(string amount, string expected)
    {
        var a = new Amount(amount);
        Assert.Equal(decimal.Parse(amount, CultureInfo.GetCultureInfo("en-US")), a.Value);
        Assert.Equal(expected, a.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00")]
    [InlineData("-")]
    [InlineData("+1")]
    [InlineData("0.")]
    [InlineData("01")]
    [InlineData("1,25")]
    public void invalid(string amount)
    {
        var _ = Assert.Throws<ArgumentException>(() => new Amount(amount));
    }
}