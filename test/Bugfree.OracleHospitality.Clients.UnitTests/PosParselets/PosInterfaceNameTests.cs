using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class PosInterfaceNameTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("42")]
    [InlineData("test")]
    public void valid(string name)
    {
        var s = new PosInterfaceName(name);
        Assert.Equal(name, s.Value);
        Assert.Equal(name, s.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void invalid(string name)
    {
        var _ = Assert.Throws<ArgumentException>(() => new PosInterfaceName(name));
    }
}