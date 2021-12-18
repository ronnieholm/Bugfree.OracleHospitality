using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class ResultSetDefinitionTests
{
    [Fact]
    public void valid()
    {
        const string element = @"
                <ResultSetDefinition>
                   <Column>firstname</Column>
                   <Column>lastname</Column>
                </ResultSetDefinition>";

        var d = new ResultSetDefinition(XE.Parse(element));
        Assert.Equal(2, d.Columns.Count);
        Assert.Equal("firstname", d.Columns[0].Value);
        Assert.Equal("lastname", d.Columns[1].Value);
    }
}