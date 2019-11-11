using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class LocalDate : IRequestElement
    {
        public DateTime Value { get; }
        public LocalDate(string value) => Value = FieldTypes.AssertDate(value);
        public LocalDate(DateTime timestamp) => Value = timestamp;
        public override string ToString() => Value.ToString("yyyyMMdd");
    }
}