using System;
using Xunit;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using static Bugfree.OracleHospitality.Clients.ConversionHelpers;

namespace Bugfree.OracleHospitality.Clients.UnitTests;

// Cleaned up code linked from https://docs.microsoft.com/en-us/sql/t-sql/functions/newsequentialid-transact-sql?view=sqlallproducts-allversions.
// Direct link: https://blogs.msdn.microsoft.com/dbrowne/2012/07/03/how-to-generate-sequential-guids-for-sql-server-in-net.
public static class SqlSequentialIdGenerator
{
    [DllImport("rpcrt4.dll", SetLastError = true)]
    static extern int UuidCreateSequential(out Guid guid);

    public static (Guid, Guid) NewSequentialId()
    {
        UuidCreateSequential(out var original);
        var s = original.ToByteArray();
        var t = new byte[16];
        t[3] = s[0];
        t[2] = s[1];
        t[1] = s[2];
        t[0] = s[3];
        t[5] = s[4];
        t[4] = s[5];
        t[7] = s[6];
        t[6] = s[7];
        t[8] = s[8];
        t[9] = s[9];
        t[10] = s[10];
        t[11] = s[11];
        t[12] = s[12];
        t[13] = s[13];
        t[14] = s[14];
        t[15] = s[15];
        return (original, new Guid(t));
    }
}

public class ConversionHelpersTests
{
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "o7wt7mrm8rj4t0j24twi3")]
    [InlineData("ec5d23e7-c543-4ba7-8d2d-13d3f3918aa6", "e1yfnbgzziponndv6pxa7")]
    [InlineData("24310721-0ad9-e811-a963-000d3ab496c3", "02o9txkc1gfilmygynaqo")] // left-padded with "0"
    public void sequential_guid_to_account_number(string input, string output)
    {
        var guid = Guid.Parse(input);
        var number = GuidToAccountNumber(guid);
        Assert.Equal(21, number.Length);
        Assert.Equal(output, number);
    }

    [Theory]
    [ClassData(typeof(RandomGuid))]
    public void account_number_must_be_21_digits(Guid guid)
    {
        var number = GuidToAccountNumber(guid);
        Assert.Equal(21, number.Length);
    }

    [Theory]
    [InlineData("ec5d23e7-c543-4ba7-8d2d-13d3f3918aa6", "54g3hKv7Xr1CuefeBp5gVh")]
    [InlineData("76d5debd-43c2-448b-b7c8-876c0d6e6c09", "0hMtYJDCf65hTPYKIuN8Gp")] // left-padded with "0"
    public void guid_to_long_alphabet(string input, string output)
    {
        var guid = Guid.Parse(input);
        var number = GuidToTraceId(guid);
        Assert.Equal(22, number.Length);
        Assert.Equal(output, number);
    }

    [Theory]
    [ClassData(typeof(RandomGuid))]
    public void trace_id_must_be_22_digits(Guid guid)
    {
        var number = GuidToTraceId(guid);
        Assert.Equal(22, number.Length);
    }

    [Theory]
    [InlineData(1, new[] { 1 }, "1")]
    [InlineData(15, new[] { 15 }, "f")]
    [InlineData(254, new[] { 15, 14 }, "fe")]
    [InlineData(255, new[] { 15, 15 }, "ff" )]
    [InlineData(3470241, new[] { 3, 4, 15, 3, 10, 1 }, "34f3a1")]
    public void base_10_to_hex(int base10, int[] hexDigits, string hexEncoded)
    {
        var digits = IntegerToBase(new BigInteger(base10), 16);
        var encoded = DigitsToEncoding(digits, ShortAlphabet);
        Assert.Equal(hexDigits, digits);
        Assert.Equal(hexEncoded, encoded);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void positive_numbers_supported(int input)
    {
        var e = Assert.Throws<ArgumentException>(() => IntegerToBase(new BigInteger(input), 2));
        Assert.Contains("number must be positive", e.Message);
    }

    [Fact]
    public void ensure_approximately_uniform_distribution_of_sha1_hashed_sequential_guids()
    {
        // P/Invoke into rpcrt4.dll is only supported on Windows. On other
        // platforms, the test results in "System.DllNotFoundException:
        // Unable to load shared library 'rpcrt4.dll' or one of its
        // dependencies".
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        const int sampleSize = 100000;
        var sha1s = new BigInteger[sampleSize]; // 2^160 states
        var _108bits = new BigInteger[sampleSize]; // 2^36 states

        // If we partition elements in a state space of size 2^m into
        // buckets of size 2^n, then the size of each bucket becomes 2^m /
        // 2^n = 2^(m - n). We may then determine bucket as modulo bucket
        // size.
        var buckets = (int)Math.Pow(2, 4);
        var sha1Buckets = new int[buckets];
        var sha1Increment = BigInteger.Pow(2, 160 - 4);
        var _108bitsBuckets = new int[buckets];
        var _108bitsIncrement = BigInteger.Pow(2, 108 - 4);

        for (var i = 0; i < sampleSize; i++)
        {
            var (_, shifted) = SqlSequentialIdGenerator.NewSequentialId();
            sha1s[i] = new BigInteger(ComputeSha1(shifted.ToByteArray()), isUnsigned: true);
            _108bits[i] = sha1s[i] >> 52;

            var sha1Idx = (int)BigInteger.Divide(sha1s[i], sha1Increment);
            sha1Buckets[sha1Idx]++;

            var _108bitIdx = (int)BigInteger.Divide(_108bits[i], _108bitsIncrement);
            _108bitsBuckets[_108bitIdx]++;
        }

        // Because we're scaling one set of numbers by a power of two,
        // outcome is also an integer and its bucket position is retained.
        Assert.Equal(sha1Buckets, _108bitsBuckets);

        var avg = sha1Buckets.Average();
        var sum = sha1Buckets.Sum(d => Math.Pow(d - avg, 2));
        var stdDiv = Math.Sqrt(sum / buckets);

        const int heuristicThreshold = 150;
        Assert.True(stdDiv < heuristicThreshold);
    }

    [Fact]
    public void base_to_base_10()
    {
        var n = BaseToInteger(new[] { 1, 2, 3 }, 10);
        Assert.Equal(new BigInteger(123), n);
    }

    [Theory]
    [ClassData(typeof(RandomNumberBase))]
    public void inverse_property(BigInteger number, int base_)
    {
        var n = BaseToInteger(IntegerToBase(number, base_), base_);
        Assert.Equal(n, number);
    }

    public class RandomNumberBase : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                var number = rng.Next(0, int.MaxValue);
                var base_ = rng.Next(2, Math.Min(number, 1000));
                yield return new object[] { new BigInteger(number), base_ };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class RandomGuid : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            for (var i = 0; i < 100; i++)
                yield return new object[] { Guid.NewGuid() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}