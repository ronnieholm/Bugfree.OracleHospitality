using System;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class ActionsTests
{
    [Fact]
    public void ValidActionsXml()
    {
        var actions = XE.Parse(@"
            <Actions>
                <Action>
                    <Type tid=""12"">Accept Coupon</Type>
                    <Data pid=""9"">1004019</Data>
                    <Code>10DKK</Code>
                    <Text>Coupon: 10 DKK, Always Valid</Text>
                </Action>
            </Actions>");

        var r = new Actions(actions);
        Assert.Single(r.Values);

        var a = r.Values[0];
        Assert.Equal(ActionType.TransactionId.AcceptCoupon, a.Type.Id);
        Assert.Equal(ActionData.PromptId.PleaseEnterCoupon, a.Data.Id);
        Assert.Equal("1004019", a.Data.Value.Value);
        Assert.Equal("10DKK", a.Code.Value);
        Assert.Equal("Coupon: 10 DKK, Always Valid", a.Text.Value.Value);
    }

    [Fact]
    public void invalid_actions()
    {
        var actions = XE.Parse("<Actions></Actions>");
        var e = Assert.Throws<ArgumentException>(() => new Actions(actions));
        Assert.Contains("Expected at least one 'Action' element", e.Message);
    }
}