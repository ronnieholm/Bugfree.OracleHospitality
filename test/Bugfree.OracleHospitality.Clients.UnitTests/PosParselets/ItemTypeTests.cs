using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class ItemTypeTests
{
    [Theory]
    [InlineData("M", ItemType.Kind.MenuOrSalesItem)]
    public void valid(string type, ItemType.Kind expected)
    {
        var kind = new ItemType(type);
        Assert.Equal(expected, kind.Value);
        Assert.Equal(type, kind.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X")]
    [InlineData("NotExist")]
    public void invalid(string type)
    {
        var _ = Assert.Throws<ArgumentException>(() => new ItemType(type));
    }
}