using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.CrmParselets;

public class DisplayMessageTests
{
    [Theory]
    [InlineData("com.micros.storedValue.worker.SetRollbackException: Update failed for row ID = 123")]
    public void valid(string message)
    {
        var dm = new DisplayMessage(message);
        Assert.Equal(message, dm.Value);
        Assert.Equal(message, dm.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void invalid(string message)
    {
        var _ = Assert.Throws<ArgumentException>(() => new DisplayMessage(message));            
    }
}