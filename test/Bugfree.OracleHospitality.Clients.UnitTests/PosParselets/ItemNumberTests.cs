using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class ItemNumberTests
{
    [Theory]
    [InlineData("42")]
    public void valid(string number)
    {
        var n = new ItemNumber(number);
        Assert.Equal(42, n.Value);
        Assert.Equal("42", n.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("-42")]
    public void invalid(string number)
    {
        var _ = Assert.Throws<ArgumentException>(() => new ItemNumber(number));
    }
}