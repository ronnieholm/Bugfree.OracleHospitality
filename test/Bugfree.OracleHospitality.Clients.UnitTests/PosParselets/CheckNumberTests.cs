using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class CheckNumberTests
    {
        [Theory]
        [InlineData("0", "0000")]
        [InlineData("0000", "0000")]
        public void valid(string number, string expected)
        {
            var cn = new CheckNumber(number);
            Assert.Equal(int.Parse(number), cn.Value);
            Assert.Equal(expected, cn.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        public void invalid(string number)
        {
            var _ = Assert.Throws<ArgumentException>(() => new CheckNumber(number));
        }

        [Fact]
        public void overflow_causes_exception()
        {
            var c = new CheckNumber(CheckNumber.MaxValue);
            var e = Assert.Throws<ArgumentException>(() => c.Increment());
            Assert.Contains("overflow", e.Message);
        }
    }
}