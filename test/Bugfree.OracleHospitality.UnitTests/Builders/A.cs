namespace Bugfree.OracleHospitality.Clients.UnitTests.Builders
{
    // DESIGN: see http://natpryce.com/articles/000714.html for A pattern.

    public static class A
    {
        public static OracleHospitalityClientOptionsBuilder OracleHospitalityClientOptions =>
            new OracleHospitalityClientOptionsBuilder();
    }
}