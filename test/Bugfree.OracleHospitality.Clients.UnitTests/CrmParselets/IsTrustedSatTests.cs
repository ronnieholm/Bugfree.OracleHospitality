using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class IsTrustedSatTests
{
    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void valid(string trusted)
    {
        var its = new IsTrustedSat(trusted);
        Assert.Equal(bool.Parse(trusted), its.Value);
        Assert.Equal(trusted, its.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("True")]
    [InlineData("False")]
    [InlineData("NotBool")]
    public void invalid(string trusted)
    {
        var _ = Assert.Throws<ArgumentException>(() => new IsTrustedSat(trusted));
    }
}