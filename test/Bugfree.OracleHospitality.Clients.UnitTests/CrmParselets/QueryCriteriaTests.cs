using Bugfree.OracleHospitality.Clients.CrmParselets;
using Xunit;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class QueryCriteriaTests
{
    [Fact]
    public void valid()
    {
        const string element = @"
                <QueryCriteria conditions=""primaryposref = ?"">
                    <Condition name=""primaryposref"" value=""123"" />
                </QueryCriteria>";

        var criteria = new QueryCriteria(XE.Parse(element));
        Assert.Equal("primaryposref = ?", criteria.Conditions_.Value);
        Assert.Single(criteria.ConditionDetails);
        Assert.Equal("primaryposref", criteria.ConditionDetails[0].Name.Value);
        Assert.Equal("123", criteria.ConditionDetails[0].Value_.Value_);
    }
}