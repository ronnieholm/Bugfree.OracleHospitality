using System;
using Xunit;
using System.Globalization;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class ExchangeRateTests
    {
        [Theory]
        [InlineData("1.00")]
        public void valid(string rate)
        {
            var er = new ExchangeRate(rate);
            Assert.Equal(decimal.Parse(rate, CultureInfo.GetCultureInfo("en-US")), er.Value);
            Assert.Equal(rate, er.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("-1.00")]
        public void invalid(string rate)
        {
            var _ = Assert.Throws<ArgumentException>(() => new ExchangeRate(rate));
        }
    }
}