using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class TransmissionTests
    {
        [Theory]
        [InlineData("n", Transmission.Kind.Normal)]
        public void valid(string transmission, Transmission.Kind expected)
        {
            var kind = new Transmission(transmission);
            Assert.Equal(expected, kind.Value);
            Assert.Equal(transmission, kind.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("x")]
        public void invalid(string transmission)
        {
            var _ = Assert.Throws<ArgumentException>(() => new Transmission(transmission));
        }
    }
}