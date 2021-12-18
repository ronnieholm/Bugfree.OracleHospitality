using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Bugfree.OracleHospitality.Clients.Seedwork;
using Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;

namespace Bugfree.OracleHospitality.Clients.UnitTests;

public class TimeProviderTestScopeTests
{
    [Fact]
    public void single_threading_returns_time_set()
    {
        var testNow = DateTime.Parse("2018-01-01T12:00:00");
        using (new TimeProviderTestScope(() => testNow))
        {
            Assert.Equal(testNow, TimeProvider.Now);
        }
    }

    private async Task TwoParallelTestsAsync(int delayMilliseconds, int timeoutMilliseconds)
    {
        using var mutex = new AutoResetEvent(false);
        var t1 = Task.Factory.StartNew(() =>
        {
            var testNow = DateTime.Parse("2018-01-01T12:00:00");
            using (new TimeProviderTestScope(() => testNow))
            {
                Assert.Equal(testNow, TimeProvider.Now);
                mutex.Set();
                Thread.Sleep(delayMilliseconds);
                Assert.Equal(testNow, TimeProvider.Now);
            }
        });

        var t2 = Task.Factory.StartNew(() =>
        {
            mutex.WaitOne();
            var testNow = DateTime.Parse("2018-02-01T12:00:00");
            using (new TimeProviderTestScope(() => testNow, timeoutMilliseconds))
            {
                Assert.Equal(testNow, TimeProvider.Now);
            }
        });

        await t1;
        await t2;
    }

    [Fact]
    public async Task two_threads_does_not_interfere_with_each_other()
    {
        await TwoParallelTestsAsync(5000, 10000);
    }

    [Fact]
    public async Task two_threads_does_interfere_with_each_other()
    {
        // Set t2's timeout less than t1's wait period to provoke a lock
        // exception.
        var e = await Assert.ThrowsAsync<Exception>(() => TwoParallelTestsAsync(5000, 1000));
        Assert.Contains("Lock is held", e.Message);
    }
}