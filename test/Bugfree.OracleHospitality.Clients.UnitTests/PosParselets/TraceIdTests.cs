using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using Bugfree.OracleHospitality.Clients.PosParselets;
using static Bugfree.OracleHospitality.Clients.PosParselets.TraceId;

// See property based testing:
// John Hughes - Testing the Hard Stuff and Staying Sane
// https://www.youtube.com/watch?v=zi0rHwfiX1Q
// - "Don't write tests. Generate them"

namespace Bugfree.OracleHospitality.Clients.UnitTests.PosParselets;

public class TraceIdTests
{
    [Theory]
    [InlineData("190715132542N010001")]
    [InlineData("000715132542N010001")]
    public void valid(string id)
    {
        var tid = new TraceId(id);
        Assert.Equal(id, tid.ToString());
    }

    [Theory]
    [InlineData("1")] // Too short
    [InlineData("190715132542X0100010")] // Too long
    [InlineData("191315132542N010001")] // Invalid month
    public void invalid(string id)
    {
        var _ = Assert.Throws<ArgumentException>(() => new TraceId(id));
    }

    [Fact]
    public void valid_id()
    {
        var traceIdString =
            new TraceId(
                new DateTime(2019, 7, 15, 15, 26, 48),
                TransmissionKind.Normal,
                new SequenceNumber(42),
                new CheckNumber(12)).ToString();
        var traceId = new TraceId(traceIdString);
        Assert.Equal(2019, traceId.Timestamp.Year);
        Assert.Equal(7, traceId.Timestamp.Month);
        Assert.Equal(15, traceId.Timestamp.Day);
        Assert.Equal(15, traceId.Timestamp.Hour);
        Assert.Equal(26, traceId.Timestamp.Minute);
        Assert.Equal(48, traceId.Timestamp.Second);
        Assert.Equal(TransmissionKind.Normal, traceId.Kind);
        Assert.Equal(42, traceId.Sequence.Value);
        Assert.Equal(12, traceId.CheckNumber.Value);
    }

    [Theory]
    [ClassData(typeof(RandomValidTraceIdData))]
    public void inverse_traceId_property(string id)
    {
        var tid = new TraceId(id);
        var tidString = new TraceId(tid.Timestamp, tid.Kind, tid.Sequence, tid.CheckNumber).ToString();
        Assert.Equal(id, tidString);
    }

    public class RandomValidTraceIdData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                var year = rng.Next(0, 100);
                var month = rng.Next(1, 13);
                var day = rng.Next(1, DateTime.DaysInMonth(2000 + year, month));
                var hour = rng.Next(0, 24);
                var minute = rng.Next(0, 60);
                var seconds = rng.Next(0, 60);
                var retransmit = rng.Next() >= 0.5 ? "N" : "R";
                var sequence = rng.Next(0, 100);
                var checkNumber = rng.Next(0, 10000);
                yield return new object[] { $"{year:D2}{month:D2}{day:D2}{hour:D2}{minute:D2}{seconds:D2}{retransmit}{sequence:D2}{checkNumber:D4}"};
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}