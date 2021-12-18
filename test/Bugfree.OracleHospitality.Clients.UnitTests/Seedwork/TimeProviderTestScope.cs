using System;
using System.Threading;
using System.Runtime.CompilerServices;
using Bugfree.OracleHospitality.Clients.Seedwork;

namespace Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;

public class TimeProviderTestScope : IDisposable
{
    private static readonly Mutex Mutex = new();
    private static string _memberName;
    private static string _filePath;
    private static int _lineNumber;
    private bool _disposed;

    public TimeProviderTestScope(Func<DateTime> provider,
        int timeoutMilliseconds = 10000,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Mutex.WaitOne(timeoutMilliseconds))
        {
            TimeProvider.SetTimeProvider(provider);
            _memberName = memberName;
            _filePath = filePath;
            _lineNumber = lineNumber;
        }
        else
            throw new Exception(
                "Forgot to call Dispose method or was Dispose method called too late? " +
                $"Lock is held by member '{_memberName}' at '{_filePath}:{_lineNumber}");
    }

    public static TimeProviderTestScope SetTimeTo(DateTime time) => new(() => time);

    public static TimeProviderTestScope SetTimeTo(string time) =>
        SetTimeTo(DateTime.Parse(time));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            TimeProvider.ResetTimeProvider();
            _memberName = "";
            _filePath = "";
            _lineNumber = 0;
            Mutex.ReleaseMutex();
        }

        _disposed = true;
    }
}