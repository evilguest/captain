using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Core
{
    public static class Evaluator
    {
        public static (double C, double A, double NA) EstimateConsistencyAvailability(decimal initialBalance, TransactionScheduler scheduler, IEnumerable<TransferRequest> history, PartitionScheduleGenerator generator, int iterations)
        {
            if (scheduler is null)
                throw new ArgumentNullException(nameof(scheduler));

            if (history is null)
                throw new ArgumentNullException(nameof(history));

            if (generator is null)
                throw new ArgumentNullException(nameof(generator));

            if (!(iterations > 0))
                throw new ArgumentOutOfRangeException(nameof(iterations), iterations, "Iterations count must be above zero");
            var count = history.Count();
            var reference = history.SingleMachineProcess(initialBalance).ToDictionary(r=>r.Id);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var totalC = 0.0;
            var totalA = 0.0;
            var totalNA = 0.0;
            var i = 0;
            foreach(var partitionSchedule in generator.Schedules)
            {
                if (++i > iterations)
                    break;

                var r = scheduler.Play(initialBalance, history, partitionSchedule).ToList();
                double c = r.GetConsistency();
                totalC += c;
                double a = r.GetAvailability(reference);
                totalA += a;
                double na = partitionSchedule.GetPartitions().GetNetworkAvailability(history.First().TimeStamp, history.Last().TimeStamp);
                totalNA += na;
                //System.Console.WriteLine($"I: {i:3} C: {c:p2} A: {a:p2} A': {na:p2}. Initial balance: {initialBalance}, final balance: {r.Last().Balance}");
            }
            sw.Stop();
            Console.WriteLine($"We run {iterations} of {count} transactions each. Tiotal time: {sw.ElapsedMilliseconds}ms, {1_000_000.0*sw.ElapsedMilliseconds / iterations / count} ns per iteration. ");
            //System.Console.ReadKey();
            return (totalC / iterations, totalA / iterations, totalNA / iterations);
        }
    }
}
