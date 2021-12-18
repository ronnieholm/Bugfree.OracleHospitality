using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class LanguageTests
{
    [Theory]
    [InlineData("en-US")]
    public void valid(string language)
    {
        var l = new Language(language);
        Assert.Equal(language, l.Value);
        Assert.Equal(language, l.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExist")]
    public void invalid(string language)
    {
        var _ = Assert.Throws<ArgumentException>(() => new ItemType(language));
    }
}