using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class TransactionEmployeeTests
{
    [Theory]
    [InlineData("0")]
    public void valid(string employee)
    {
        var te = new TransactionEmployee(employee);
        Assert.Equal(int.Parse(employee), te.Value);
        Assert.Equal(employee, te.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotNumber")]
    public void invalid(string employee)
    {
        var _ = Assert.Throws<ArgumentException>(() => new RevenueCenter(employee));
    }
}