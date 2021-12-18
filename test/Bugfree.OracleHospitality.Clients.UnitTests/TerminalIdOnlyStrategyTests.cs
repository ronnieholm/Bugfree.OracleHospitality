using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;
using Bugfree.OracleHospitality.Clients.UnitTests.Builders;
using Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies;

namespace Bugfree.OracleHospitality.Clients.UnitTests;

public class TerminalIdOnlyStrategyTests
{
    private void AssertMessageId(TerminalId terminalId, SequenceNumber sequenceNumber, MessageId messageId)
    {
        Assert.Equal(terminalId.Value, messageId.TerminalId.Value);
        Assert.Equal(sequenceNumber.Value, messageId.SequenceNumber.Value);
    }

    // Ensures wraparound behavior with non-zero initial value. For min =
    // 100 and max = 103 (not inclusive), ids become 100, 101, 102, 100
    // (wraparound), 101, 102, ... Wraparound behavior is achieved by
    // shifting 100, 101, 102 to 0, 1, 2, then wrapping around, and then
    // shifting back to 100, 101, 102. Using this function, we can test with
    // random initial terminalId.
    private int Id(int min, int max, int value) => ((value - min) % (max - min)) + min;

    [Fact]
    public void too_many_requested_message_ids_within_grace_period()
    {
        const int minValue = 100;
        const int maxValue = 103;
        var sn = TerminalIdOnlyStrategy.ConstantSequenceNumber;
        var options = 
            A.OracleHospitalityClientOptions
                .WithTerminalIdLowerBound(minValue)
                .WithTerminalIdUpperBound(maxValue)
                .Build();
        var sut = new TerminalIdOnlyStrategy(Options.Create(options));

        MessageId initial = null;
        using (new TimeProviderTestScope(() => DateTime.Parse("2018-01-01T12:00:00")))
        {
            initial = sut.Next();
            AssertMessageId(new TerminalId(initial.TerminalId.ToString()), sn, initial);
            AssertMessageId(new TerminalId(Id(minValue, maxValue, initial.TerminalId.Value + 1).ToString()), sn, sut.Next());
            AssertMessageId(new TerminalId(Id(minValue, maxValue, initial.TerminalId.Value + 2).ToString()), sn, sut.Next());
        }

        using (new TimeProviderTestScope(() => DateTime.Parse("2018-01-01T12:15:59")))
        {
            var e = Assert.Throws<ArgumentException>(() => sut.Next());
            Assert.Contains($"TerminalId {initial.TerminalId} was last used at 20180101120000", e.Message);
        }
    }

    [Fact]
    public void reuse_message_id_when_outside_grace_period()
    {
        const int minValue = 100;
        const int maxValue = 103;
        var sn = TerminalIdOnlyStrategy.ConstantSequenceNumber;
        var options = 
            A.OracleHospitalityClientOptions
                .WithTerminalIdLowerBound(minValue)
                .WithTerminalIdUpperBound(maxValue)
                .Build();
        var sut = new TerminalIdOnlyStrategy(Options.Create(options));

        MessageId initial = null;
        using (new TimeProviderTestScope(() => DateTime.Parse("2018-01-01T12:00:00")))
        {
            initial = sut.Next();
            AssertMessageId(new TerminalId(initial.TerminalId.ToString()), sn, initial);
            AssertMessageId(new TerminalId(Id(minValue, maxValue, initial.TerminalId.Value + 1).ToString()), sn, sut.Next());
            AssertMessageId(new TerminalId(Id(minValue, maxValue, initial.TerminalId.Value + 2).ToString()), sn, sut.Next());
        }

        using (new TimeProviderTestScope(() => DateTime.Parse("2018-01-01T12:16:00")))
        {
            AssertMessageId(new TerminalId(initial.TerminalId.ToString()), sn, sut.Next());
        }
    }

    [Fact]
    public async Task multiple_threads_requesting_message_ids_receive_non_overlapping_messageIds()
    {
        // Comment out lock statement in
        // IncrementTerminalIdKeepSequenceNumberConstantStrategy class to
        // introduce a race condition that makes the test fail.
        const int minValue = 0;
        const int maxValue = 10000;
        var messageIds = new ConcurrentDictionary<int, int>();
        var options =
            A.OracleHospitalityClientOptions
                .WithTerminalIdLowerBound(minValue)
                .WithTerminalIdUpperBound(maxValue)
                .Build();
        var sut = new TerminalIdOnlyStrategy(Options.Create(options));

        var t1 = Task.Factory.StartNew(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                var id = sut.Next();
                messageIds.AddOrUpdate(id.TerminalId.Value, 1, (_, b) => b + 1);
                System.Threading.Thread.Sleep(5);
            }
        });

        var t2 = Task.Factory.StartNew(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                var id = sut.Next();
                messageIds.AddOrUpdate(id.TerminalId.Value, 1, (_, b) => b + 1);
                System.Threading.Thread.Sleep(1);
            }
        });

        await t1;
        await t2;

        Assert.Equal(2000, messageIds.Count);
        foreach (var ids in messageIds)
            Assert.Equal(1, ids.Value);
    }

    [Fact]
    public void prune_message_id_history_when_element_outside_grace_period()
    {
        var options = 
            A.OracleHospitalityClientOptions
                .WithTerminalIdLowerBound(0)
                .WithTerminalIdUpperBound(10000)
                .Build();
        var sut = new TerminalIdOnlyStrategy(Options.Create(options));
        const int threshold = TerminalIdOnlyStrategy.HistoryAdditionOperationsPruneThreshold;

        using (new TimeProviderTestScope(() => DateTime.Parse("2018-01-01T12:00:00")))
        {
            for (var i = 0; i < threshold - 1; i++)
                sut.Next();
        }

        Assert.Equal(threshold - 1, sut.History.Count);

        using (new TimeProviderTestScope(() => DateTime.Parse("2018-01-01T12:16:01")))
            sut.Next();

        Assert.Single(sut.History);
    }
}