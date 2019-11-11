using System.Linq;

namespace Bugfree.OracleHospitality.Clients
{
    public static class Crc32
    {
        // For background on CRC, see
        // https://www.youtube.com/watch?v=izG7qT0EpBw.
        //
        // CRC is used to detect burst errors and the CRC32 variant can detect
        // up to 32 mangled bits in a row. Why CRC32 is used with the Oracle
        // APIs, except for legacy reasons, is unclear. Perhaps because TCP/IP
        // wasn't always used for transport. Because with TCP/IP the IPv4, but
        // not IPv6, header itself holds a checksum protecting IP header fields
        // from corruption and within the data link layer, the Ethernet header
        // contains a CRC field.
        private readonly static uint[] ChecksumTable;

        // Whether to compute or hardcode the checksum table is a classic
        // time-space trade-off. We elect to compute the table, but only once
        // per process.
        static Crc32()
        {
            const uint GeneratorPolynomial = 0xEDB88320;
            ChecksumTable = Enumerable.Range(0, 256).Select(i =>
            {
                var tableEntry = (uint)i;
                for (var j = 0; j < 8; j++)
                {
                    tableEntry = ((tableEntry & 1) == 0)
                        ? tableEntry >> 1
                        : (GeneratorPolynomial ^ (tableEntry >> 1));
                }
                return tableEntry;
            }).ToArray();
        }

        public static uint Compute(byte[] byteStream)
        {
            // Oracle APIs assume CRC32 is Big Endian
            return ~byteStream.Aggregate(0xFFFFFFFF, (checksumRegister, currentByte) =>
                ChecksumTable[(checksumRegister & 0xFF) ^ currentByte] ^ (checksumRegister >> 8));
        }

        public static string ToPaddedCrc32String(uint crc32)
        {
            // Make sure to left-pad output so 0x762AE69 becomes 0x0762EA69,
            // left-padded with zeroes.
            return crc32.ToString("X8");
        }
    }
}