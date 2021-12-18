using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class DisplayMessageTests
{
    [Theory]
    [InlineData("Coupon (10 DKK) has been issued to this account.")]
    public void valid(string message)
    {
        var dm = new DisplayMessage(message);
        Assert.Equal(message, dm.Value);
        Assert.Equal(message, dm.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void invalid(string message)
    {
        var _ = Assert.Throws<ArgumentException>(() => new DisplayMessage(message));
    }
}