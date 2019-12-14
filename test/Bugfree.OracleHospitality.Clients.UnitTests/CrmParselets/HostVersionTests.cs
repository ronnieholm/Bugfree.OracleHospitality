using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets
{
    public class HostVersionTests
    {
        [Theory]
        [InlineData("1.00")]
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