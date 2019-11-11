using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies
{
    // Useful in testing and when client wants precise control of message
    // sequencing.
    public class FixedStrategy : IPosMessageSequencingStrategy
    {
        private readonly TerminalId _terminalId;
        private readonly SequenceNumber _sequenceNumber;
        private readonly CheckNumber _checkNumber;

        public static FixedStrategy Default() =>
            new FixedStrategy(
                new TerminalId(0),
                new SequenceNumber(0),
                new CheckNumber(0));

        public FixedStrategy(TerminalId terminalId, SequenceNumber sequenceNumber, CheckNumber checkNumber)
        {
            _terminalId = terminalId;
            _sequenceNumber = sequenceNumber;
            _checkNumber = checkNumber;
        }

        public MessageId Next() => new MessageId(_terminalId, _sequenceNumber, _checkNumber);
    }
}