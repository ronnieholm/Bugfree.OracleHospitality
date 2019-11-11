using System;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class RequestCode : IRequestElement
    {
        public enum Kind
        {
            None,
            SetCustomer,
            PostAccountTransaction,
            GetColumnList,
            GetCustomer,
            GetCoupons,
            GetAccount,
            GetProgram
        }

        public Kind Value { get; }

        public RequestCode(string value)
        {
            FieldTypes.AssertString(value);
            if (!Enum.IsDefined(typeof(Kind), value))
                throw new ArgumentException($"Unknown value '{value}'");
            Value = (Kind)Enum.Parse(typeof(Kind), value);
        }

        public RequestCode(Kind kind)
        {
            if (kind == Kind.None)
                throw new ArgumentException($"{nameof(Kind)} must not be {kind}");
            Value = kind;
        }

        public override string ToString() => Value.ToString();
    }
}