using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class BusinessDate : IRequestElement
    {
        public DateTime Value { get; }
        public BusinessDate(string value) => Value = FieldTypes.AssertDate(value);

        // UNDOCUMENTED: it's unspecified whether date should be in UTC or local
        // time as is the case with LocalDate.
        public BusinessDate(DateTime timestamp) => Value = timestamp;

        public override string ToString() => Value.ToString("yyyyMMdd");
    }
}