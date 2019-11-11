using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets
{
    public class RequestCodeTests
    {
        [Theory]
        [InlineData("GetCustomer", RequestCode.Kind.GetCustomer)]
        public void valid(string code, RequestCode.Kind expected)
        {
            var rc = new RequestCode(code);
            Assert.Equal(expected, rc.Value);
            Assert.Equal(code, rc.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("NotExist")]
        public void invalid(string code)
        {
            var _ = Assert.Throws<ArgumentException>(() => new RequestCode(code));
        }
    }
}