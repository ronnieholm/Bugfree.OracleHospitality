using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class LocalTime : IRequestElement
    {
        public TimeSpan Value { get; }
        public LocalTime(string value) => Value = FieldTypes.AssertTime(value);
        public LocalTime(DateTime timestamp) => Value = timestamp.TimeOfDay;
        public override string ToString() => Value.ToString("hhmmss");
    }
}