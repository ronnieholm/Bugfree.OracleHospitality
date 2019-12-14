using System;
using System.Threading;
using System.Runtime.CompilerServices;
using Bugfree.OracleHospitality.Clients.Seedwork;

namespace Bugfree.OracleHospitality.Clients.UnitTests.Seedwork
{
    public class TimeProviderTestScope : IDisposable
    {
        private static readonly Mutex Mutex = new Mutex();
        private static string MemberName;
        private static string FilePath;
        private static int LineNumber;
        private bool disposed;

        public TimeProviderTestScope(Func<DateTime> provider,
            int timeoutMilliseconds = 10000,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (Mutex.WaitOne(timeoutMilliseconds))
            {
                TimeProvider.SetTimeProvider(provider);
                MemberName = memberName;
                FilePath = filePath;
                LineNumber = lineNumber;
            }
            else
                throw new Exception(
                    "Forgot to call Dispose method or was Dispose method called too late? " +
                    $"Lock is held by member '{MemberName}' at '{FilePath}:{LineNumber}");
        }

        public static TimeProviderTestScope SetTimeTo(DateTime time) =>
            new TimeProviderTestScope(() => time);

        public static TimeProviderTestScope SetTimeTo(string time) =>
            SetTimeTo(DateTime.Parse(time));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                TimeProvider.ResetTimeProvider();
                MemberName = "";
                FilePath = "";
                LineNumber = 0;
                Mutex.ReleaseMutex();
            }

            disposed = true;
        }
    }
}