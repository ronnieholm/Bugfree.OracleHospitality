using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

// See also: http://bugfree.dk/blog/2019/09/18/randomizing-and-encoding-sequential-guids-in-a-larger-alphabet

namespace Bugfree.OracleHospitality.Clients
{
    public static class ConversionHelpers
    {
        public const string ShortAlphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
        public const string LongAlphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GuidToAccountNumber(Guid guid)
        {
            // Convert 128 bit Guid to 160 bit hash in case the Guid is a non-
            // random one, such a MS SQL Server's sequential Guid.
            var a = ComputeSha1(guid.ToByteArray());

            // Ensure that a becomes positive when the byte array is passed to
            // BigInteger's ctor. The array is interpreted Little Endian, so by
            // adding 0x00 at the end we ensure the sign is positive. From .NET
            // Core 2.1, we can pass isUnsigned: true to BigInteger's ctor, but
            // sticking to .NET Standard 2.0 broadens the range of consumers,
            // including .NET framework 4.8.
            var b = a.Concat(new byte[] { 0x00 }).ToArray();

            // BigInteger constructor is the sole reason this project targets
            // netcoreapp2.1 over netstandard2.0. It's unclear how to hand-roll
            // the same behavior with .NET Standard.
            var c = new BigInteger(b);

            // Discard right-most bits of SHA1 so that remaining bit length is
            // the largest number that base 36 encoded fits into 21 digits, the
            // length of an Oracle account number without preamble.
            var d = c >> 52;
            var e = IntegerToBase(d, ShortAlphabet.Length);
            var f = DigitsToEncoding(e, ShortAlphabet);

            // In principle, each of 160 bits could be zero such that when
            // converting to base 36 outcome is a single zero. To ensure account
            // numnber is always 21 digits, we left-pad encoded number. In
            // practice, SHA1 works such that we rarely end up having to pad. On
            // a sample of 1,000,000 Guids, only one ended up with 16 digits.
            return f.PadLeft(21, '0');
        }

        public static string GuidToTraceId(Guid guid)
        {
            var a = guid.ToByteArray().Concat(new byte[] { 0x00 }).ToArray();
            var b = new BigInteger(a);
            var c = IntegerToBase(b, LongAlphabet.Length);
            var d = DigitsToEncoding(c, LongAlphabet);
            return d.PadLeft(22, '0');
        }

        public static int[] IntegerToBase(BigInteger number, int base_)
        {
            if (number.Sign != 1)
                throw new ArgumentException($"{nameof(number)} must be positive. Was {number}");
            if (base_ < 2)
                throw new ArgumentException($"{nameof(base_)} must be at least two. Was {base_}");

            var alphabetPositions = new List<int>();
            while (number > 0)
            {
                var quotient = BigInteger.DivRem(number, base_, out var digit);
                alphabetPositions.Add((int)digit);
                number = quotient;
            }
            alphabetPositions.Reverse();
            return alphabetPositions.ToArray();
        }

        public static BigInteger BaseToInteger(int[] digits, int base_)
        {
            if (base_ < 2)
                throw new ArgumentException($"{nameof(base_)} must be at least two. Was {base_}");

            BigInteger value = 0;
            var j = 0;
            for (var i = digits.Length - 1; i >= 0; i--)
                value += digits[i] * BigInteger.Pow(base_, j++);

            return value;
        }

        public static string DigitsToEncoding(int[] digits, string alphabet)
        {
            var sb = new StringBuilder();
            foreach (var d in digits)
                sb.Append(alphabet[d]);
            return sb.ToString();
        }

        public static byte[] ComputeSha1(byte[] bytes)
        {
            using var sha1 = new SHA1Managed();
            return sha1.ComputeHash(bytes);
        }
    }
}