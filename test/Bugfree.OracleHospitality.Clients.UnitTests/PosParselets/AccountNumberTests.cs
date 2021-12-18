using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class AccountNumberTests
{
    [Theory]
    [InlineData("2200005")]
    [InlineData("1a2b")]
    public void valid(string account)
    {
        var a = new AccountNumber(account);
        Assert.Equal(account, a.Value);
        Assert.Equal(account, a.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("05cb2406-3b80-472a-8200-64ec4e045da2")] // Too long
    public void invalid(string account)
    {
        var _ = Assert.Throws<ArgumentException>(() => new AccountNumber(account));
    }
}