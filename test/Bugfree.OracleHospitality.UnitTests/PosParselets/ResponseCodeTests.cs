using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets
{
    public class ResponseCodeTests
    {
        [Theory]
        [InlineData("<ResponseCode>A</ResponseCode>", ResponseCode.Kind.Approved, "A")]
        public void approved_response(string response, ResponseCode.Kind kind, string code)
        {
            var k = new ResponseCode(XE.Parse(response));
            Assert.Equal(kind, k.Value);
            Assert.Equal(code, k.ToString());
        }

        [Fact]
        public void data_center_initiated_error_response()
        {
            var r = new ResponseCode(XE.Parse(@"<ResponseCode hostCode=""20"">D</ResponseCode>"));
            Assert.Equal(ResponseCode.Kind.DataCenterInitiatedError, r.Value);
            Assert.Equal("20", r.HostCode);
        }
    }
}