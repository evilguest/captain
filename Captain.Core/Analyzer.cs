using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Core
{
    public static class Analyzer
    {
        public static IEnumerable<TransferResult> SingleMachineProcess(this IEnumerable<TransferRequest> requests)
        {
            var balance = 0m;
            foreach(var r in requests)
            {
                var confirm = (balance + r.Amount) >= 0;
                if (confirm)
                    balance += r.Amount;
                yield return new TransferResult(r, confirm);
            }
        }
        public static IEnumerable<TransferResult> UltimateAcceptanceProcess(this IEnumerable<TransferRequest> requests)
        {
            var balance = 0m;
            foreach (var r in requests)
            {
                balance += r.Amount;
                yield return new TransferResult(r, true);
            }
        }

        public static double GetConsistency(this IEnumerable<TransferResult> results)
        {
            var positive = new TimeSpan();
            var negative = new TimeSpan();
            var balance = 0m;
            // we need to count share of the time account was negative
            foreach(var s in results.Zip(results.Skip(1), (r1, r2)=>(Duration: r2.TimeStamp-r1.TimeStamp, Balance: balance+=r1.Confirmed?r1.Amount:0)))
            {
                if (s.Balance >= 0)
                    positive += s.Duration;
                else
                    negative += s.Duration;
            }

            return positive / (positive + negative);
        }
        public static double GetAvailability(this IEnumerable<TransferResult> results, IEnumerable<TransferResult> referenceResults)
        {
            int accepted = 0;
            int rejected = 0;
            foreach (var rr in results.Zip(referenceResults))
            {
                if (rr.First.Id != rr.Second.Id)
                    throw new InvalidOperationException($"Mismatch between results sequence, {rr.First.Id} != {rr.Second.Id}");

                if (rr.First.Confirmed)
                {
                    if (rr.Second.Confirmed)
                        accepted++;
                    else
                        // oops! Inconsistency!!!
                        continue;
                }
                else
                {
                    if (rr.Second.Confirmed)
                        rejected++;
                }
            }
            return ((double)accepted / (accepted + rejected));
        }

        //public static double GetAvailability(this IEnumerable<TransferResult> results, IDictionary<string, TransferResult> referenceResults)
        //{
        //    int accepted = 0;
        //    int rejected = 0;
        //    foreach(var r in results)
        //    {
        //        if (r.Confirmed)
        //        {
        //            accepted++;
        //            if (!referenceResults[r.Id].Confirmed)
        //                // oops! Inconsistency!!!
        //                continue;
        //        }
        //        else 
        //        { 
        //            if (referenceResults[r.Id].Confirmed)
        //                rejected++;
        //        }
        //    }
        //    return ((double)accepted / (accepted + rejected));
        //}
        public static double GetNetworkAvailability(this IEnumerable<Partition> partitions, DateTimeOffset start, DateTimeOffset end)
        {
            TimeSpan on = new TimeSpan();
            TimeSpan off = new TimeSpan();
            var lastPartitionFinish = start;
            foreach (var partition in partitions)
            {
                if (partition.start > end)
                    break;
                on += (partition.start - lastPartitionFinish);
                lastPartitionFinish = (partition.finish > end) ? end : partition.finish;
                off += lastPartitionFinish - partition.start;
            }
            on += (end - lastPartitionFinish);
            return on / (on + off);
        }
    }
}
