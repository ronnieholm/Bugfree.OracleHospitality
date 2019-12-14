using System;
using Xunit;
using System.Globalization;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class BalanceTests
    {
        [Theory]
        [InlineData("0")]
        [InlineData("0.00")]
        public void valid(string balance)
        {
            var b = new Balance(balance);
            Assert.Equal(decimal.Parse(balance, CultureInfo.GetCultureInfo("en-US")), b.Value);
            Assert.Equal(balance, b.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        public void invalid(string balance)
        {
            var _ = Assert.Throws<ArgumentException>(() => new Amount(balance));
        }
    }
}