using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Core
{
    public static class Evaluator
    {
        public static (double C, double A, double NA) EstimateConsistencyAvailability(string name, Func<IRequestHandler> factory, IEnumerable<TransferRequest> history, PartitionScheduleGenerator generator, int iterations, IStatisticsWriter writer)
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            if (history is null)
                throw new ArgumentNullException(nameof(history));

            if (generator is null)
                throw new ArgumentNullException(nameof(generator));

            if (!(iterations > 0))
                throw new ArgumentOutOfRangeException(nameof(iterations), iterations, "Iterations count must be above zero");
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var count = history.Count();
            var reference = history.SingleMachineProcess();//.ToDictionary(r=>r.Id);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var totalC = 0.0;
            var totalA = 0.0;
            var totalP = 0.0;
            var i = 0;
            foreach(var partitionSchedule in generator.Schedules)
            {
                if (++i > iterations)
                    break;

                var r = Play(history, partitionSchedule, factory()).ToList();
                double c = r.GetConsistency();
                totalC += c;
                double a = r.GetAvailability(reference);
                totalA += a;
                double p = partitionSchedule.Partitions.GetNetworkAvailability(history.First().TimeStamp, history.Last().TimeStamp);
                totalP += p;
                writer.WriteResult(new StatisticsItem(name, c, a, p));
            }
            sw.Stop();
            Console.WriteLine($"We run {iterations} of {count} transactions each. Total time: {sw.ElapsedMilliseconds}ms, {1_000_000.0*sw.ElapsedMilliseconds / iterations / count} ns per iteration. ");
            //System.Console.ReadKey();
            return (totalC / iterations, totalA / iterations, totalP / iterations);
        }

        public static IEnumerable<TransferResult> Play(IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule, IRequestHandler h)
        {
            if (requests is null)
                throw new ArgumentNullException(nameof(requests));

            if (partitionSchedule is null)
                throw new ArgumentNullException(nameof(partitionSchedule));
            return PlayImpl(requests, partitionSchedule, h);
        }

        private static IEnumerable<TransferResult> PlayImpl(IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule, IRequestHandler h)
        {
            using var requestEnumerator = requests.GetEnumerator();
            using var partitionEnumerator = partitionSchedule.Partitions.GetEnumerator();
            if (!requestEnumerator.MoveNext())
                yield break; // nothing to return
            var request = requestEnumerator.Current;

            while (partitionEnumerator.MoveNext())
            {
                while (request.TimeStamp < partitionEnumerator.Current.start) // no partition yet
                {
                    yield return h.ProcessRequest(request);
                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                    request = requestEnumerator.Current;
                }

                // distribute the balances across the nodes according to the sync policy
                h.StartPartition();

                while (request.TimeStamp < partitionEnumerator.Current.finish) // in partition
                {

                    yield return h.ProcessRequest(request);

                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                    request = requestEnumerator.Current;
                }
                // merge the balances back to the total one
                h.FinishPartition();
            }

            while (requestEnumerator.MoveNext())
            {
                yield return h.ProcessRequest(request);

                if (!requestEnumerator.MoveNext())
                    yield break; // done
                request = requestEnumerator.Current;
            }

        }

    }
}
