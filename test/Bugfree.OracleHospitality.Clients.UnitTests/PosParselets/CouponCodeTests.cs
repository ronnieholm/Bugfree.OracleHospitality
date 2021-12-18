using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class CouponCodeTests
{
    [Theory]
    [InlineData("10DKK")]
    public void valid(string code)
    {
        var cc = new CouponCode(code);
        Assert.Equal(code, cc.Value);
        Assert.Equal(code, cc.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void invalid(string code)
    {
        var _ = Assert.Throws<ArgumentException>(() => new CouponCode(code));
    }
}