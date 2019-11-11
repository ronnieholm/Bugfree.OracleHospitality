﻿using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients
{
    public class MessageId
    {
        public TerminalId TerminalId { get; }
        public SequenceNumber SequenceNumber { get; }
        public CheckNumber CheckNumber { get; }

        public MessageId(TerminalId terminalId, SequenceNumber sequenceNumber, CheckNumber checkNumber)
        {
            TerminalId = terminalId;
            SequenceNumber = sequenceNumber;
            CheckNumber = checkNumber;
        }
    }

    // DESIGN: according to POS API spec, Page 14, Message sequencing, the
    // (terminalId, sequenceNumber) tuple shouldn't be reused within a 15
    // minutes sliding window. It would cause rollback of the previous
    // tranaction for (terminalId, sequenceNumber) and application of the
    // current transaction. If both previous and current transaction does the
    // same thing, such as COUPON_INQUERY then no harm is done. If we previously
    // did an ISSUE_COUPON, rolling back the previous transaction and issue
    // would be undesired. The exact rollback nature is undocumented.
    public interface IPosMessageSequencingStrategy
    {
        MessageId Next();
    }
}