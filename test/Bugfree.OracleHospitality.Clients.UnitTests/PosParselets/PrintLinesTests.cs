using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class PrintLinesTests
{
    [Fact]
    public void multiple_print_lines_response()
    {
        var element = XE.Parse(@"
                <PrintLines>
                    <PrintLine>Foo</PrintLine>
                    <PrintLine>Bar</PrintLine>
                </PrintLines>");

        var r = new PrintLines(element);
        Assert.Equal(2, r.Values.Count);
        Assert.Equal("Foo", r.Values[0].Value);
        Assert.Equal("Bar", r.Values[1].Value);
    }

    [Fact]
    public void empty_print_lines_response()
    {
        var element = XE.Parse("<PrintLines></PrintLines>");
        var r = new PrintLines(element);
        Assert.Empty(r.Values);
    }
}