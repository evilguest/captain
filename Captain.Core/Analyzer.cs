using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Core
{
    public static class Analyzer
    {
        public static IEnumerable<TransferResult> SingleMachineProcess(this IEnumerable<TransferRequest> requests, decimal initialBalance)
        {
            var balance = initialBalance;
            foreach(var r in requests)
            {
                var confirm = (balance + r.Amount) >= 0;
                if (confirm)
                    balance += r.Amount;
                yield return new TransferResult()
                {
                    TimeStamp = r.TimeStamp,
                    Amount = r.Amount,
                    Balance = balance,
                    Confirmed = confirm
                };
            }
        }

        public static double GetConsistency(this IEnumerable<TransferResult> results)
        {
            TimeSpan positive = new TimeSpan();
            TimeSpan negative = new TimeSpan();
            // we need to count share of the time account was negative
            foreach(var s in results.Zip(results.Skip(1), (r1, r2)=>(Duration: r2.TimeStamp-r1.TimeStamp, r1.Balance)))
            {
                if (s.Balance >= 0)
                    positive += s.Duration;
                else
                    negative += s.Duration;
            }

            return positive / (positive + negative);
        }
        public static double GetAvailability(this IEnumerable<TransferResult> results, IDictionary<DateTimeOffset, TransferResult> referenceResults)
        {
            int accepted = 0;
            int rejected = 0;
            foreach(var r in results)
            {
                if (referenceResults[r.TimeStamp].Confirmed && !r.Confirmed)
                    rejected++;
                else
                    accepted++;
            }
            return ((double)accepted / (accepted + rejected));
        }
        public static double GetNetworkAvailability(this IEnumerable<(DateTimeOffset start, TimeSpan duration)> partitions, DateTimeOffset start, DateTimeOffset end)
        {
            TimeSpan on = new TimeSpan();
            TimeSpan off = new TimeSpan();
            var lastPartitionEnd = start;
            foreach (var partition in partitions)
            {
                on += (partition.start - lastPartitionEnd);
                off += partition.duration;
            }
            on += (end - lastPartitionEnd);
            return on / (on + off);
        }
    }
}
