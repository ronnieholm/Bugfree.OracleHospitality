using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class ErrorTests
{
    [Fact]
    public void valid()
    {
        const string element = @"<Error code=""1"">Unsupported parameter: NonExistingEntity</Error>";
        var error = new Error(XE.Parse(element));

        Assert.Equal("1", error.Code);
        Assert.Equal("Unsupported parameter: NonExistingEntity", error.Message);
    }
}