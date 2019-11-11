using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class TraceId : IRequestElement, IResponseElement
    {
        // TransmissionKind for TraceId is different from the retransmit
        // attribute parsed by the Transmission class. It has values 'n' and 'y'
        // that map to normal and retransmit whereas TraceId maps 'N' and 'R' to
        // normal and retransmit.
        public enum TransmissionKind
        {
            None,
            Normal,
            Retransmit
        }

        public DateTime Timestamp { get; }
        public TransmissionKind Kind { get; }

        // UNDOCUMENTED: it's unclear whether sequence number and check number
        // as part of TraceId should be the same as sequence attribute and
        // checkNumber elements or why they would be different.
        public SequenceNumber Sequence { get; }
        public CheckNumber CheckNumber { get; }

        public TraceId(string value)
        {
            // UNDOCUMENTED: while POS API spec, Page 23 designates TraceID as
            // A25, examples show it as A19. Possibly for Oracle to change
            // TraceID in the future without updating the schema.
            FieldTypes.AssertA25(value);

            // Date part of TraceID doesn't follow the pattern used with
            // LocalDate and BusinessDate. For TraceID, year is two characters
            // only. Since TraceID is an element on its own and not a field type
            // (Ax, Nx, ...), its regular expression is defined inline and not
            // with FieldTypes.
            const string pattern = @"^(?<year>\d{2})(?<month>\d{2})(?<day>\d{2})(?<hour>\d{2})(?<minute>\d{2})(?<second>\d{2})(?<retransmit>.{1})(?<sequence>\d{2})(?<checkNumber>\d{4})$";
            var re = new Regex(pattern);
            var m = re.Match(value);
            if (!m.Success)
                throw new ArgumentException($"{nameof(value)} must match pattern: {pattern}. Was '{value}'");

            var date = FieldTypes.AssertDate($"20{m.Groups["year"].Value}{m.Groups["month"].Value}{m.Groups["day"].Value}");
            var time = FieldTypes.AssertTime($"{m.Groups["hour"].Value}{m.Groups["minute"].Value}{m.Groups["second"].Value}");
            Timestamp = date.Add(time);

            var retransmit = m.Groups["retransmit"].Value;
            if (!(retransmit == "N" || retransmit == "R"))
                throw new ArgumentException($"Retransmit must be either 'N' or 'R'. Was '{retransmit}'");
            Kind = retransmit == "N" ? TransmissionKind.Normal : TransmissionKind.Retransmit;

            var sequence = int.Parse(m.Groups["sequence"].Value);
            Sequence = new SequenceNumber(sequence);
            var checkNumber = int.Parse(m.Groups["checkNumber"].Value);
            CheckNumber = new CheckNumber(checkNumber);
        }

        public TraceId(DateTime timestamp, TransmissionKind transmissionKind, SequenceNumber sequence, CheckNumber checkNumber)
        {
            Timestamp = timestamp;
            Kind = transmissionKind;
            Sequence = sequence;
            CheckNumber = checkNumber;
        }

        public override string ToString()
        {
            // UNDOCUMENTED: POS API spec doesn't state timezone for TraceID. We
            // assume local time over UTC. Beware that if we change to UTC,
            // tests using TimeProviderTestScope will fail because they're
            // initialized from a DateTime string without timezone information.
            var traceIdBuffer = new StringBuilder(Timestamp.ToString("yyMMddHHmmss"), 19);
            var transmissionKind = Kind switch
            {
                TransmissionKind.Normal => "N",
                TransmissionKind.Retransmit => "R",
                _ => throw new ArgumentException($"Unsupported {nameof(TransmissionKind)}: {Kind}"),
            };

            traceIdBuffer.Append(transmissionKind);
            traceIdBuffer.Append(Sequence.ToString());
            traceIdBuffer.Append(CheckNumber.ToString());
            return traceIdBuffer.ToString();
        }
    }
}