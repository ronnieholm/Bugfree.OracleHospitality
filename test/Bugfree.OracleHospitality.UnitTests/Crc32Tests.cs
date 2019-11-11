using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xunit;

// Use https://crccalc.com for manual verification.

namespace Bugfree.OracleHospitality.Clients.UnitTests
{
    public class Crc32Tests
    {
        [Theory]
        [ClassData(typeof(FixedMessages))]
        public void fixed_messages(string message, uint expectedCrc)
        {
            var actual = Crc32.Compute(Encoding.UTF8.GetBytes(message));
            Assert.Equal(expectedCrc, actual);
        }

        [Theory]
        [ClassData(typeof(RandomMessages))]
        public void random_messages(byte[] message, uint selfCrc, uint referenceCrc)
        {
            var actual = Crc32.Compute(message);
            Assert.Equal(selfCrc, actual);
            Assert.Equal(selfCrc, referenceCrc);
            Assert.Equal(8, Crc32.ToPaddedCrc32String(selfCrc).Length);
        }

        [Theory]
        [InlineData(0x0762EA69, "0762EA69")]
        public void padding_on_left_zero(uint crc, string formattedCrc)
        {
            var actual = Crc32.ToPaddedCrc32String(crc);
            Assert.Equal(formattedCrc, actual);
        }

        public class FixedMessages : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "", 0x00000000 };
                yield return new object[] { "123456789", 0xCBF43926 };
                yield return new object[] { "Hello World", 0x4A17B156 };

                // A non-7-bit ASCII character turns into bytes 0xC2, 0xBD bytes
                // when it's UTF8 encoded.
                yield return new object[] { "½", 0x0DC56802 };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class RandomMessages : IEnumerable<object[]>
        {
            private readonly Random _rng = new Random();
           
            public IEnumerator<object[]> GetEnumerator()
            {
                for (var i = 0; i < 100; i++)
                {
                    // Don't set message size too high or the generation of
                    // random bytes will slow down test.
                    var sizeOfArray = _rng.Next(0, 1024 + 1);
                    var message = new byte[sizeOfArray];
                    _rng.NextBytes(message);
                    yield return new object[] 
                    { 
                        message, 
                        Crc32.Compute(message),
                        Force.Crc32.Crc32Algorithm.Compute(message)
                    };
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}