using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class CurrencyTests
{
    [Theory]
    [InlineData("USD")]
    public void valid(string currency)
    {
        var c = new Currency(currency);
        Assert.Equal(Currency.Kind.USD, c.Value);
        Assert.Equal(currency, c.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void invalid(string currency)
    {
        var _ = Assert.Throws<ArgumentException>(() => new Currency(currency));
    }
}