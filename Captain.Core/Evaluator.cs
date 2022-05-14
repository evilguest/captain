using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Core
{
    public static class Evaluator
    {
        public static (double C, double A, double NA) EstimateConsistencyAvailability(ITransactionScheduler scheduler, IEnumerable<TransferRequest> history, PartitionScheduleGenerator generator, int iterations, IStatisticsWriter writer)
        {
            if (scheduler is null)
                throw new ArgumentNullException(nameof(scheduler));

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

                var r = scheduler.Play(history, partitionSchedule).ToList();
                double c = r.GetConsistency();
                totalC += c;
                double a = r.GetAvailability(reference);
                totalA += a;
                double p = partitionSchedule.Partitions.GetNetworkAvailability(history.First().TimeStamp, history.Last().TimeStamp);
                totalP += p;
                writer.WriteResult(new StatisticsItem(scheduler.Name, c, a, p));
            }
            sw.Stop();
            Console.WriteLine($"We run {iterations} of {count} transactions each. Total time: {sw.ElapsedMilliseconds}ms, {1_000_000.0*sw.ElapsedMilliseconds / iterations / count} ns per iteration. ");
            //System.Console.ReadKey();
            return (totalC / iterations, totalA / iterations, totalP / iterations);
        }
    }
}
