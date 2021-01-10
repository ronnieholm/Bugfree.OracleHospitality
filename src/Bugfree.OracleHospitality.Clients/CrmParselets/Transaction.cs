using System;
using Bugfree.OracleHospitality.Clients.Seedwork;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class Transaction : IRequestElement
    {
        public abstract class TransactionBuilder
        {
            // For now, all we need is the ability to Close and Reopen accounts.
            // If we require additional transaction types, factor common
            // elements and behavior into this case.
            //
            // Each transaction is akin to a sub-operation of
            // PostAccountTransaction.

            public abstract XE Build();
        }

        // UNDOCUMENTED: the elements that must go with kind CloseAccount and
        // ReopenAccount aren't part of the CRM API spec, Page 33. They were
        // inferred by starting with Type and for each Oracle error message
        // about a missing element, the element was added.
        public class CloseReopenTransactionBuilder : TransactionBuilder
        {
            private Type _type;
            private CustomerFriendlyDescription _description;
            private ProgramCode _programCode;
            private AccountPosRef _accountPosRef;
            private TransactionDateTime _transactionDateTime;
            private BusinessDate _businessDate;
            private CardPresent _cardPresent;
            private Currency _currency;
            private TraceId _traceId;

            public CloseReopenTransactionBuilder()
            {
                var now = TimeProvider.Now;
                _description = new CustomerFriendlyDescription($"{nameof(CloseReopenTransactionBuilder).Replace("Builder", "")}");
                _transactionDateTime = new TransactionDateTime(now);
                _businessDate = new BusinessDate(now);
                _cardPresent = new CardPresent(false);
                _currency = new Currency(Currency.Kind.DKK);
                _traceId = new TraceId(Guid.NewGuid());
            }

            public CloseReopenTransactionBuilder WithType(Type.Kind kind)
            {
                if (!(kind == Type.Kind.CloseAccount || kind == Type.Kind.ReopenAccount))
                    throw new ArgumentException($"Only {Type.Kind.CloseAccount} or {Type.Kind.ReopenAccount} supported. Was {kind}");
                _type = new Type(kind);
                return this;
            }

            public CloseReopenTransactionBuilder WithCustomerFriendlyDescription(string description)
            {
                _description = new CustomerFriendlyDescription(description);
                return this;
            }

            public CloseReopenTransactionBuilder WithProgramCode(string code)
            {
                _programCode = new ProgramCode(code);
                return this;
            }

            public CloseReopenTransactionBuilder WithAccountPosRef(string accountNumber)
            {
                _accountPosRef = new AccountPosRef(accountNumber);
                return this;
            }

            public CloseReopenTransactionBuilder WithTransactionDateTime(DateTime timestamp)
            {
                _transactionDateTime = new TransactionDateTime(timestamp);
                return this;
            }

            public CloseReopenTransactionBuilder WithBusinessDate(DateTime timestamp)
            {
                _businessDate = new BusinessDate(timestamp);
                return this;
            }

            public CloseReopenTransactionBuilder WithCardPresent(bool present)
            {
                _cardPresent = new CardPresent(present);
                return this;
            }

            public CloseReopenTransactionBuilder WithCurrency(Currency.Kind kind)
            {
                _currency = new Currency(kind);
                return this;
            }

            public CloseReopenTransactionBuilder WithTraceId(Guid traceId)
            {
                _traceId = new TraceId(traceId);
                return this;
            }

            public CloseReopenTransactionBuilder WithTraceId(string traceId)
            {
                _traceId = new TraceId(traceId);
                return this;
            }

            public override XE Build()
            {
                return
                    new XE(C.Transaction,
                        new XE(C.Type, _type),
                        new XE(C.CustFriendlyDesc, _description),
                        new XE(C.ProgramCode, _programCode),
                        new XE(C.AccountPOSRef, _accountPosRef),
                        new XE(C.TransDateTime, _transactionDateTime),
                        new XE(C.BusinessDate, _businessDate),
                        new XE(C.CardPresent, _cardPresent),
                        new XE(C.LocalCurrencyISOCode, _currency),
                        new XE(C.TraceID, _traceId));
            }
        }

        public class Type : IRequestElement
        {
            // From CRM API spec, Page 33.
            public enum Kind
            {
                None,
                CloseAccount = 5,
                ReopenAccount = 6
            }

            public Kind Value { get; }

            public Type(string value)
            {
                FieldTypes.AssertString(value);
                if (!Enum.IsDefined(typeof(Kind), value))
                    throw new ArgumentException($"Unknown value '{value}'");
                Value = (Kind)Enum.Parse(typeof(Kind), value);
            }

            public Type(Kind kind)
            {
                if (kind == Kind.None)
                    throw new ArgumentException($"{nameof(Kind)} must not be {kind}");
                Value = kind;
            }

            public override string ToString() => ((int)Value).ToString();
        }

        public class CustomerFriendlyDescription : IRequestElement
        {
            public string Value { get; }
            public CustomerFriendlyDescription(string value) => Value = FieldTypes.AssertString(value);
            public override string ToString() => Value;
        }

        public class ProgramCode : IRequestElement
        {
            public string Value { get; }
            public ProgramCode(string value) => Value = FieldTypes.AssertString(value);
            public override string ToString() => Value;
        }

        public class AccountPosRef : IRequestElement
        {
            public string Value { get; }
            public AccountPosRef(string value) => Value = FieldTypes.AssertString(value);
            public override string ToString() => Value;
        }

        // UNDOCUMENTED: CRM API spec leaves format unspecified, but includes
        // examples of the form "2004-08-12 18:36:33.0". 
        public class TransactionDateTime : IRequestElement
        {
            public DateTime Value { get; }
            public TransactionDateTime(string value) => Value = FieldTypes.AssertTimestamp(value);
            public TransactionDateTime(DateTime value) => Value = value;
            public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss.f");
        }

        // UNDOCUMENTED: CRM API spec leaves exact format unspecified, but
        // includes examples of the form "2004-08-12".
        public class BusinessDate : IRequestElement
        {
            public DateTime Value { get; }
            public BusinessDate(string value) => Value = FieldTypes.AssertTimestamp(value);
            public BusinessDate(DateTime value) => Value = value;
            public override string ToString() => Value.ToString("yyyy-MM-dd");
        }

        public class CardPresent : IRequestElement
        {
            public bool Value { get; }
            public CardPresent(string value) => Value = FieldTypes.AssertBoolean(value);
            public CardPresent(bool value) => Value = value;
            public override string ToString() => Value ? "1" : "0";
        }

        public class TraceId : IRequestElement
        {
            public string Value { get; }
            public TraceId(string value) => Value = FieldTypes.AssertString(value);
            public TraceId(Guid value) => Value = ConversionHelpers.GuidToTraceId(value);
            public override string ToString() => Value;
        }
    }
}