using System;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Bugfree.OracleHospitality.Clients.Seedwork;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies
{
    // DESIGN: preferred strategy with multi-threading though may as well be
    // used with single-threading for low friction setup. For instance, CLI may
    // be launched many times over a 15 minute sliding window and each time it
    // requires unique messageIds. When using this strategy make sure dependency
    // injection container is setup to inject a single instance of this class
    // into PosClient. In ConfigureServices, setup must be asingleton:
    //
    // .AddSingleton<IPosMessageSequencingStrategy, TerminalIdOnlyStrategy>()
    //
    // Even using this strategy of terminal id hopping (inspired by frequency
    // hopping spread spectrum), the Oracle backend has another, possibly
    // orthogonal mechanism for tracking request. So while this strategy
    // protects against rollback and cross-talk, it doesn't mean that we can
    // repidly issue more than 9,999 requests. Duing batch load of accounts into
    // GL, after at most 9,999 requsts, we end with an error response like this
    // one:
    //
    // <ResponseCode hostCode="68">D</ResponseCode>
    // <DisplayMessage>Only one card can be associated with a check</DisplayMessage>
    //
    // Presumably, the Oracle backend has logic that within a window of time
    // tracks the last account number associated with a check number, and a
    // check number must be in the range [0;9999]. The account number/check
    // number tracking is independant of terminal id used.

    public class TerminalIdOnlyStrategy : IPosMessageSequencingStrategy
    {
        public const int HistoryAdditionOperationsPruneThreshold = 1000;
        private static TimeSpan ReuseGracePeriod = new TimeSpan(0, /* add one minute to be safe from rounding */ 16, 0);

        public TerminalId LowerBound { get; private set; }
        public TerminalId UpperBound { get; private set; }
        public static SequenceNumber ConstantSequenceNumber = new SequenceNumber(0);

        // Holds for at least the grace period MessageIds to detect duplicates
        // It may hold additional MessageIds until
        // HistoryAdditionOperationsCleanupThreshold is reaches and dictionary
        // is pruned to hold only MessageIds within the grace period.
        public ConcurrentDictionary<int, DateTime> History { get; private set; }

        private int _historyAdditionOperationsUntilNextPrune = HistoryAdditionOperationsPruneThreshold;
        private MessageId _nextMessageId;
        private object _mutex = new object();

        public TerminalIdOnlyStrategy(IOptions<OracleHospitalityClientsOptions> options)
        {
            var o = options.Value;
            var lower = o.PointOfSaleOperations.TerminalIdLowerBound;
            var upper = o.PointOfSaleOperations.TerminalIdUpperBound;
            var randomInitial = new Random().Next(lower, upper);
            Initialize(lower, upper, randomInitial);
        }

        private void Initialize(int lowerBound, int upperBound, int initial)
        {
            if (initial < lowerBound || initial >= upperBound)
                throw new ArgumentException($"Initial TerminalId {initial} must not be less in range [{lowerBound};{upperBound}[ ");

            LowerBound = new TerminalId(lowerBound.ToString());
            UpperBound = new TerminalId(upperBound.ToString());
            History = new ConcurrentDictionary<int, DateTime>();
            _nextMessageId =
                new MessageId(
                    new TerminalId(initial.ToString()),
                    ConstantSequenceNumber,
                    new CheckNumber(initial % CheckNumber.MaxValue));
        }

        public MessageId Next()
        {
            lock (_mutex)
            {
                var terminalId = _nextMessageId.TerminalId.Value;
                var success = History.TryGetValue(terminalId, out var timestamp);
                if (success && timestamp.Add(ReuseGracePeriod) > TimeProvider.Now)
                {
                    // Explicitly format timestamp or it'll render differently
                    // based on machine setup, Not only does it make tests fail
                    // but reading the message in a log one would have to make
                    // assumptions about the format.
                    throw new ArgumentException($"TerminalId {terminalId} was last used at {timestamp.ToString("yyyyMMddHHmmss")} which is less than {ReuseGracePeriod.Minutes} minutes ago. Consider increasing the terminalId pool size");
                }

                var current = _nextMessageId;
                History[current.TerminalId.Value] = TimeProvider.Now;
                _historyAdditionOperationsUntilNextPrune--;

                // Pruning history whenever it reaches a certain length could
                // lead to cleaning having little effect on length when only a
                // single MessageId expired since the last call. Instead we
                // clean only every n operations. This means that we may store a
                // longer history than the grace period requires, but that
                // cleaning is triggered less frequently and with larger effect
                // on length. 
                if (_historyAdditionOperationsUntilNextPrune == 0)
                {
                    var clean = History.Where(kvp => kvp.Value.Add(ReuseGracePeriod) < TimeProvider.Now).ToArray();
                    foreach (var entry in clean)
                        History.TryRemove(entry.Key, out _);
                    _historyAdditionOperationsUntilNextPrune = HistoryAdditionOperationsPruneThreshold;
                }

                var nextTerminalId =
                    current.TerminalId.Value + 1 == UpperBound.Value
                    ? LowerBound.Value
                    : current.TerminalId.Value + 1;
                _nextMessageId =
                    new MessageId(
                        new TerminalId(nextTerminalId.ToString()),
                        ConstantSequenceNumber,
                        new CheckNumber(nextTerminalId % CheckNumber.MaxValue));
                return current;
            }
        }
    }
}