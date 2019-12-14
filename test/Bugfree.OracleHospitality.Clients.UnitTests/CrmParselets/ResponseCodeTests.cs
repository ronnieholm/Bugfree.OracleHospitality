using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets
{
    public class ResponseCodeTests
    {
        [Theory]
        [InlineData("A", ResponseCode.Kind.Approved)]
        [InlineData("E", ResponseCode.Kind.Error)]
        public void valid(string code, ResponseCode.Kind expected)
        {
            var kind = new ResponseCode(code);
            Assert.Equal(expected, kind.Value);
            Assert.Equal(code, kind.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("NotExist")]
        public void invalid(string code)
        {
            var _ = Assert.Throws<ArgumentException>(() => new ResponseCode(code));
        }
    }
}