using System;
using Xunit;
using XE = System.Xml.Linq.XElement;
using XA = System.Xml.Linq.XAttribute;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.UnitTests;

public class ParserHelpersTests
{
    [Fact]
    public void expect_attribute_success_when_attribute_present()
    {
        var e = XE.Parse(@"<e a=""123"" />");
        var a = ExpectAttribute(e, "a");
        Assert.Equal("123", a.Value);
    }

    [Fact]
    public void expect_attribute_failure_when_attribute_missing()
    {
        var e = XE.Parse(@"<e a=""123"" />");
        var ae = Assert.Throws<ArgumentException>(() => ExpectAttribute(e, "b"));
        Assert.Equal("Expected 'b' attribute", ae.Message);
    }

    [Fact]
    public void expect_element_success_when_element_present()
    {
        var e = XE.Parse(@"
            <e>
                <e1>42</e1>
            </e>");
        var e1 = ExpectElement(e, "e1");
        Assert.Equal("42", e1.Value);
    }

    [Fact]
    public void expect_element_failure_when_element_missing()
    {
        var e = XE.Parse(@"
            <e>
                <e1>42</e1>
            </e>");
        var ae = Assert.Throws<ArgumentException>(() => ExpectElement(e, "e2"));
        Assert.Equal("Expected 'e2' element", ae.Message);
    }

    [Fact]
    public void expect_elements_success_when_collection_present()
    {
        var e = XE.Parse(@"
            <e>
                <e1>42</e1>
                <e1>43</e1>
            </e>");
        var e1s = ExpectElements(e, "e1");
        Assert.Equal(2, e1s.Length);
        Assert.Equal("42", e1s[0].Value);
        Assert.Equal("43", e1s[1].Value);
    }

    [Fact]
    public void expect_elements_fail_when_collection_missing()
    {
        var e = XE.Parse(@"
            <e>
                <e1>42</e1>
            </e>");
        var ae = Assert.Throws<ArgumentException>(() => ExpectElements(e, "e2"));
        Assert.Equal("Expected at least one 'e2' element", ae.Message);
    }

    [Fact]
    public void consume_attribute_success_when_attribute_present()
    {
        var e = XE.Parse(@"<e a=""123"" />");
        ConsumeAttribute(e, "a");
        Assert.Null(e.Attribute("a"));
    }

    [Fact]
    public void consume_attribute_fail_when_attribute_missing()
    {
        var e = XE.Parse(@"<e a=""123"" />");
        var ae = Assert.Throws<ArgumentException>(() => ConsumeAttribute(e, "a2"));
        Assert.Equal("Expected 'a2' attribute", ae.Message);
    }

    [Fact]
    public void consume_element_success_when_element_present()
    {
        var e = XE.Parse(@"
            <e>
                <e1>42</e1>
            </e>");
        ConsumeElement(e, "e1");
        Assert.Equal("<e />", e.ToString());
    }

    [Fact]
    public void consume_element_fail_when_element_missing()
    {
        var e = XE.Parse(@"
            <e>
                <e1>42</e1>
            </e>");
        var ae = Assert.Throws<ArgumentException>(() => ConsumeElement(e, "e2"));
        Assert.Equal("Expected 'e2' element", ae.Message);
    }

    [Fact]
    public void map_attribute_success_when_attribute_present()
    {
        var e = XE.Parse(@"<e a=""123"" />");
        XA a = null;
        MapAttribute(e, "a", x => a = x);
        Assert.Equal("123", a.Value);
    }

    [Fact]
    public void map_attribute_fail_when_attribute_missing()
    {
        var e = XE.Parse(@"<e a=""123"" />");
        XA a = null;
        var ae = Assert.Throws<ArgumentException>(() => MapAttribute(e, "a2", x => a = x));
        Assert.Equal("Expected 'a2' attribute", ae.Message);
    }

    [Fact]
    public void map_element_success_when_element_present()
    {
        var e = XE.Parse("<e><e1>42</e1></e>");
        XE el = null;
        MapElement(e, "e1", x => el = x);
        Assert.Equal("42", el.Value);
    }

    [Fact]
    public void map_element_fail_when_element_mssing()
    {
        var e = XE.Parse("<e><e1>42</e1></e>");
        XE el = null;
        var ae = Assert.Throws<ArgumentException>(() => MapElement(e, "e2", x => el = x));
        Assert.Equal("Expected 'e2' element", ae.Message);
    }

    [Fact]
    public void validate_attribute_success_when_attribute_present()
    {
        var e1 = XE.Parse(@"<e a=""123"" />");
        var e2 = XE.Parse(@"<e a=""123"" />");

        var run = false;
        ValidateAttribute(e1, e2, "a", (a, b) =>
        {
            run = true;
            return a == b;
        });

        Assert.True(run);
    }

    [Fact]
    public void validate_attribute_fail_when_attribute_missing()
    {
        var e1 = XE.Parse(@"<e a=""123"" />");
        var e2 = XE.Parse(@"<e a=""1234"" />");

        var run = false;
        var e = Assert.Throws<OracleHospitalityClientException>(() =>
            ValidateAttribute(e1, e2, "a", (a, b) =>
            {
                run = true;
                return a == b;
            }));

        Assert.True(run);
        Assert.Equal("Expected attribute values for 'a' to be equal. Was '123' and '1234'", e.Message);
    }

    [Fact]
    public void validate_element_success_when_element_present()
    {
        var e1 = XE.Parse("<e1><e2>123</e2></e1>");
        var e2 = XE.Parse("<e1><e2>123</e2></e1>");

        var run = false;
        ValidateElement(e1, e2, "e2", (a, b) =>
        {
            run = true;
            return a == b;
        });

        Assert.True(run);
    }

    [Fact]
    public void validate_element_fail_when_element_missing()
    {
        var e1 = XE.Parse("<e1><e2>123</e2></e1>");
        var e2 = XE.Parse("<e1><e2>1234</e2></e1>");

        var run = false;
        var e = Assert.Throws<OracleHospitalityClientException>(() =>
            ValidateElement(e1, e2, "e2", (a, b) =>
            {
                run = true;
                return a == b;
            }));

        Assert.True(run);
        Assert.Equal("Expected element values for 'e2' to be equal. Was '123' and '1234'", e.Message);
    }
}