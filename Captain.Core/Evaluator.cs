using System.Collections.Generic;
using System.Linq;

namespace Captain.Core
{
    public class Evaluator
    {
        public (double C, double A, double NA) EstimateConsistencyAvailability(decimal initialBalance, TransactionScheduler scheduler, IEnumerable<TransferRequest> history, PartitionScheduleGenerator generator, int iterations)
        {
            var reference = history.SingleMachineProcess(initialBalance).ToDictionary(r=>r.Id);

            var partitionSchedules = generator.Schedules.GetEnumerator();
            var totalC = 0.0;
            var totalA = 0.0;
            var totalNA = 0.0;
            for (var i = 0; i < iterations; i++)
            {
                partitionSchedules.MoveNext();
                var partitionSchedule = partitionSchedules.Current;
                var r = scheduler.Play(initialBalance, history, partitionSchedule).ToList();
                double c = r.GetConsistency();
                totalC += c;
                double a = r.GetAvailability(reference);
                totalA += a;
                double na = partitionSchedule.GetPartitions().GetNetworkAvailability(history.First().TimeStamp, history.Last().TimeStamp);
                totalNA += na;
                //System.Console.WriteLine($"I: {i:3} C: {c:p2} A: {a:p2} A': {na:p2}");
            }
            return (totalC / iterations, totalA / iterations, totalNA / iterations);
        }
    }
}
