using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class ProgramNameTests
    {
        [Theory]
        [InlineData("Employee Discount 20%")]
        public void valid(string name)
        {
            var pn = new ProgramName(name);
            Assert.Equal(name, pn.Value);
            Assert.Equal(name, pn.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void invalid(string name)
        {
            var _ = Assert.Throws<ArgumentException>(() => new ProgramName(name));
        }
    }
}