using System;
using Xunit;
using Version = Bugfree.OracleHospitality.Clients.PosParselets.Version;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class VersionTests
{
    [Theory]
    [InlineData("1")]
    public void valid(string version)
    {
        var v = new Version(version);
        Assert.Equal(version, v.Value);
        Assert.Equal(version, v.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void invalid(string version)
    {
        var _ = Assert.Throws<ArgumentException>(() => new Version(version));
    }
}