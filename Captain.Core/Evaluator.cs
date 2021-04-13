using System.Collections.Generic;
using System.Linq;

namespace Captain.Core
{
    public class Evaluator
    {
        public (double C, double A) EstimateConsistencyAvailability(decimal initialBalance, TransactionScheduler scheduler, IEnumerable<TransferRequest> history, PartitionScheduleGenerator generator, int iterations)
        {
            var reference = history.SingleMachineProcess(initialBalance).ToDictionary(r=>r.TimeStamp);

            var partitionSchedules = generator.Schedules.GetEnumerator();
            var c = 0.0;
            var a = 0.0;
            for (var i = 0; i < iterations; i++)
            {
                var partitionSchedule = partitionSchedules.Current;
                partitionSchedules.MoveNext();
                var r = scheduler.Play(initialBalance, history, partitionSchedule);
                c += r.GetConsistency();
                a += r.GetAvailability(reference);
            }
            return (c / iterations, a / iterations);
        }
    }
}
