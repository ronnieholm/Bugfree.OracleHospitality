using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class HostVersionTests
    {
        [Theory]
        [InlineData("9.1.0000.2301")]
        public void valid(string version)
        {
            var hv = new HostVersion(version);
            Assert.Equal(version, hv.Value);
            Assert.Equal(version, hv.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void invalid(string version)
        {
            var _ = Assert.Throws<ArgumentException>(() => new HostVersion(version));
        }
    }
}