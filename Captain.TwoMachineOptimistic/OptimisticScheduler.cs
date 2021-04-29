using Captain.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Captain
{
    public class OptimisticScheduler: TransactionScheduler
    {
        private readonly int _seed;
        private int _machineCount;
        public OptimisticScheduler(int seed, int machineCount)
        {
            _seed = seed;
            _machineCount = (machineCount > 0) ? machineCount : throw new ArgumentOutOfRangeException(nameof(machineCount), _machineCount, "Machine count must be above zero");
        }
        public override IEnumerable<TransferResult> Play(decimal initialBalance, IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule)
        {
            if (requests is null)
                throw new ArgumentNullException(nameof(requests));

            if (partitionSchedule is null)
                throw new ArgumentNullException(nameof(partitionSchedule));

            var r = new Random(_seed);
            var balance = initialBalance;
            var balances = new decimal[_machineCount];
            using var requestEnumerator = requests.GetEnumerator();
            using var partitionEnumerator = partitionSchedule.GetPartitions().GetEnumerator();
            if (!requestEnumerator.MoveNext())
                yield break; // nothing to return

            while (partitionEnumerator.MoveNext())
            {
                while (requestEnumerator.Current.TimeStamp < partitionEnumerator.Current.start) // no partition yet
                {
                    var request = requestEnumerator.Current;
                    var confirmed = (balance + request.Amount >= 0);
                    if (confirmed)
                        balance += request.Amount;

                    yield return new TransferResult(request.Id)
                    {
                        TimeStamp = request.TimeStamp,
                        Amount = request.Amount,
                        Balance = balance,
                        Confirmed = confirmed,
                    };
                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                }

                // init all balances to the last seen syncronized value
                for (var i = 0; i < balances.Length; i++)
                    balances[i] = balance;

                while (requestEnumerator.Current.TimeStamp < partitionEnumerator.Current.start + partitionEnumerator.Current.duration) // in partition
                {
                    var request = requestEnumerator.Current;

                    // randomly choose the target host
                    var machine = r.Next(_machineCount);

                    var confirmed = (balances[machine] + request.Amount >= 0);
                    if (confirmed)
                        balances[machine] += request.Amount;

                    yield return new TransferResult(request.Id)
                    {
                        TimeStamp = request.TimeStamp,
                        Amount = request.Amount,
                        Balance = balances[machine],
                        Confirmed = confirmed,
                    };

                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                }
                // merge the balances back to the total one
                decimal delta = 0;
                for (var i = 0; i < balances.Length; i++)
                    delta += balances[i] - balance;
                balance += delta;
            }
            while (requestEnumerator.MoveNext())
            {

            }
        }
    }
}
