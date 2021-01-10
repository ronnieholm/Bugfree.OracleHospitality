using System;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class ResponseCode : IResponseElement
    {
        public enum Kind
        {
            None,
            Approved,
            DataCenterInitiatedError,
            Error
        }

        public Kind Value { get; }

        /*
         On success response contains

             <ResponseCode>A</ResponseCode>

         On failure response contains

             <ResponseCode>E</ResponseCode>
             <Error code="1">Unsupported parameter: NonExistingEntity</Error>

         Or for SetCustomer operation (among others):

            <ResponseCode>D</ResponseCode>
            <DisplayMessage>com.micros.storedValue.worker.SetRollbackException: Update failed for row ID = 123</DisplayMessage>
        */
        public ResponseCode(string value)
        {
            FieldTypes.AssertString(value);
            Value = value switch
            {
                "A" => Kind.Approved,
                "D" => Kind.DataCenterInitiatedError,
                "E" => Kind.Error,
                _ => throw new ArgumentException($"Unsupported {nameof(Kind)} value: '{value}'")
            };
        }

        public ResponseCode(Kind kind)
        {
            if (kind == Kind.None)
                throw new ArgumentException($"{nameof(Kind)} must not be {kind}");
            Value = kind;
        }

        public override string ToString()
        {
            return Value switch
            {
                Kind.Approved => "A",
                Kind.DataCenterInitiatedError => "D",
                Kind.Error => "E",
                _ => throw new ArgumentException($"Unsupported {nameof(Kind)}: '{Value}'")
            };
        }
    }
}