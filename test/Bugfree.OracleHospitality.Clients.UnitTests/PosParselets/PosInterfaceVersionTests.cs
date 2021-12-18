using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class PosInterfaceVersionTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("42")]
    [InlineData("test")]
    public void valid(string version)
    {
        var s = new PosInterfaceVersion(version);
        Assert.Equal(version, s.Value);
        Assert.Equal(version, s.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void invalid(string version)
    {
        var _ = Assert.Throws<ArgumentException>(() => new PosInterfaceVersion(version));
    }
}