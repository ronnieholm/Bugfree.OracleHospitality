using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace Bugfree.OracleHospitality.Clients.UnitTests.Seedwork
{
    public static class TestHelpers
    {
        public static HttpMessageHandler CreateMockMessageHandler(HttpStatusCode statusCode, string content)
        {
            // https://stackoverflow.com/questions/36425008/mocking-httpclient-in-unit-tests
            var mock = new Mock<HttpMessageHandler>();
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
            return mock.Object;
        }

        public static string CreateSoapResponse(string content)
        {
            return $@"<S:Envelope xmlns:S=""http://schemas.xmlsoap.org/soap/envelope/"">
               <S:Body>
                 <ns2:processRequestResponse xmlns:ns2=""ejb.storedValue.micros.com"">
                   <return>{WebUtility.HtmlEncode(content)}</return>
                 </ns2:processRequestResponse>
               </S:Body>
             </S:Envelope>";
        }
    }
}