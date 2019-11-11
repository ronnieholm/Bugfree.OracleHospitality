using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class RequestCodeTests
    {
        [Theory]
        [InlineData("COUPON_INQUIRY", TransactionKind.COUPON_INQUIRY)]
        public void valid(string code, TransactionKind expected)
        {
            var kind = new RequestCode(code);
            Assert.Equal(expected, kind.Value);
            Assert.Equal(expected.ToString(), kind.ToString());
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