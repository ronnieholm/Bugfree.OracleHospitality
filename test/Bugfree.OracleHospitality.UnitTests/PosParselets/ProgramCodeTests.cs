using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class ProgramCodeTests
    {
        [Theory]
        [InlineData("EMPDISC")]
        public void valid(string code)
        {
            var c = new ProgramCode(code);
            Assert.Equal(code, c.Value);
            Assert.Equal(code, c.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12345678901234567")]
        public void invalid(string code)
        {
            var _ = Assert.Throws<ArgumentException>(() => new ProgramCode(code));
        }
    }
}