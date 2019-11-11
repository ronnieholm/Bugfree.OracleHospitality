using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class SequenceNumberTests
    {
        [Theory]
        [InlineData("0", "00")]
        [InlineData("00", "00")]
        public void valid(string number, string expected)
        {
            var sn = new SequenceNumber(number);
            Assert.Equal(expected, sn.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        public void invalid(string number)
        {
            var _ = Assert.Throws<ArgumentException>(() => new SequenceNumber(number));
        }

        [Fact]
        public void overflow_causes_rollover()
        {
            var sn = new SequenceNumber(SequenceNumber.MaxValue);
            var sn2 = sn.Increment();
            Assert.Equal("01", sn2.ToString());
        }
    }
}