using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class TerminalTypeTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("abc")]
    public void valid(string type)
    {
        var tt = new TerminalType(type);
        Assert.Equal(type, tt.Value);
        Assert.Equal(type, tt.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123456789")]
    public void invalid(string type)
    {
        var _ = Assert.Throws<ArgumentException>(() => new TerminalType(type));
    }
}