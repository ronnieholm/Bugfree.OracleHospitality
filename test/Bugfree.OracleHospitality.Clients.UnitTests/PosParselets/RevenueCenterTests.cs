using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class RevenueCenterTests
{
    [Theory]
    [InlineData("0")]
    public void valid(string center)
    {
        var rc = new RevenueCenter(center);
        Assert.Equal(int.Parse(center), rc.Value);
        Assert.Equal(center, rc.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotNumber")]
    public void invalid(string center)
    {
        var _ = Assert.Throws<ArgumentException>(() => new RevenueCenter(center));
    }
}