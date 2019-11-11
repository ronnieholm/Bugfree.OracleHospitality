using System;

namespace Bugfree.OracleHospitality.Clients.Seedwork
{
    // See http://bugfree.dk/blog/2019/08/26/controlling-time-from-unit-tests-with-dotnet-core
    public static class TimeProvider
    {
        private static readonly Func<DateTime> DefaultProvider = () => DateTime.UtcNow;
        private static Func<DateTime> Provider = DefaultProvider;
        public static DateTime Now => Provider();

        public static void SetTimeProvider(Func<DateTime> providerFn) =>
            Provider = providerFn ?? throw new ArgumentNullException(nameof(providerFn));

        public static void ResetTimeProvider() => Provider = DefaultProvider;
    }
}