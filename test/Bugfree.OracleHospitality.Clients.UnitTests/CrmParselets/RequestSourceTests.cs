using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class RequestSourceTests
{
    [Theory]
    [InlineData(@"<RequestSource name=""acme.com"" />", "acme.com")]
    public void valid(string source, string name)
    {
        var rs = new RequestSource(XE.Parse(source));
        Assert.Equal(name, rs.Name_.Value);
        Assert.Equal(name, rs.Name_.ToString());
    }

    [Theory]
    [InlineData(@"<RequestSource name="""" />")]
    public void invalid(string source)
    {
        var _ = Assert.Throws<ArgumentException>(() => new RequestSource(XE.Parse(source)));
    }
}