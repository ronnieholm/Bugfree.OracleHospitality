using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class CurrencyTests
{
    [Theory]
    [InlineData("DKK")]
    public void valid(string currency)
    {
        var c = new Currency(currency);
        Assert.Equal(Currency.Kind.DKK, c.Value);
        Assert.Equal(currency, c.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("CURRENCYDONOTEXIST")]
    public void invalid(string currency)
    {
        var _ = Assert.Throws<ArgumentException>(() => new Currency(currency));
    }
}