using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets
{
    public class LanguageTests
    {
        [Theory]
        [InlineData("USD")]
        public void valid(string language)
        {
            var l = new Language(language);
            Assert.Equal(language, l.Value);
            Assert.Equal(language, l.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void invalid(string language)
        {
            var _ = Assert.Throws<ArgumentException>(() => new Language(language));
        }
    }
}