using System;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Version = Bugfree.OracleHospitality.Clients.PosParselets.Version;
using XA = System.Xml.Linq.XAttribute;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.PosOperations
{
    public abstract class PosRequest
    {
        protected DateTime Timestamp { get; }
        protected MessageId MessageId { get; }
        protected AccountNumber AccountNumber { get; }

        public abstract XE /* In0 */ BuildRequestDocument();

        protected PosRequest(DateTime timestamp, MessageId messageId, AccountNumber accountNumber)
        {
            if (timestamp == default)
                throw new ArgumentException(nameof(timestamp));

            Timestamp = timestamp;
            MessageId = messageId ?? throw new ArgumentNullException(nameof(messageId));
            AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        }

        protected XE BuildBaseDocument(RequestCode requestCode)
        {
            // UNDOCUMENTED: example in POS API spec, Appendix II on Page 53
            // shows the presence of a timeout attribute which we could set to
            // equal the timeout period for HttpClient. The attribute isn't part
            // of the table on Page 30 so we ignore it. According to Oracle,
            // it's likely leftover from when backend was JBoss based.
            var traceId = new TraceId(Timestamp, TraceId.TransmissionKind.Normal, MessageId.SequenceNumber, MessageId.CheckNumber).ToString();
            return new XE(C.SVCMessage,
                new XA(C.version, new Version("1")),
                new XA(C.posIntfcName, new PosInterfaceName("posInterfaceName")),
                new XA(C.posIntfcVersion, new PosInterfaceVersion("1.00")),
                new XA(C.language, new Language(Language.Kind.EnUs)),
                new XA(C.currency, new Currency(Currency.Kind.DKK)),
                new XA(C.sequence, MessageId.SequenceNumber),
                new XA(C.retransmit, new Transmission(Transmission.Kind.Normal)),
                new XE(C.RequestCode, requestCode),
                new XE(C.TraceID, traceId),
                new XE(C.TerminalID, MessageId.TerminalId),
                new XE(C.TerminalType, new TerminalType("Service")),
                new XE(C.LocalDate, new LocalDate(Timestamp)),
                new XE(C.LocalTime, new LocalTime(Timestamp)),
                new XE(C.Amount, new Amount(0.00m)),
                new XE(C.LocalCurrency, new Currency(Currency.Kind.DKK)),
                new XE(C.BusinessDate, new BusinessDate(Timestamp)),
                new XE(C.TransactionEmployee, new TransactionEmployee(0)),
                new XE(C.RevenueCenter, new RevenueCenter(0)),
                new XE(C.CheckNumber, MessageId.CheckNumber),
                new XE(C.SVAN, AccountNumber));
        }
    }
}