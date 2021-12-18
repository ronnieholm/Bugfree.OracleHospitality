using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class TerminalIdTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("42")]
    public void valid(string id)
    {
        var tid = new TerminalId(id);
        Assert.Equal(int.Parse(id), tid.Value);
        Assert.Equal(id, tid.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567890")]
    public void invalid(string id)
    {
        var _ = Assert.Throws<ArgumentException>(() => new TerminalId(id));
    }
}