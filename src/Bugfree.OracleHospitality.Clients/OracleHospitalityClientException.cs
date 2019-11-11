using System;

namespace Bugfree.OracleHospitality.Clients
{
    // Oracle Hospitality clients boundary exception
    public class OracleHospitalityClientException : Exception
    {
        public string Code { get; }

        public OracleHospitalityClientException() { }
        public OracleHospitalityClientException(string message, Exception innerException): base(message, innerException) { }
        public OracleHospitalityClientException(string message) : base(message) { }
        public OracleHospitalityClientException(string code, string message) : base(message) => Code = code;
    }
}