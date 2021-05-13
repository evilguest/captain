using Captain.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public abstract class MultiNodeSchedulerBase : TransactionScheduler
    {
        protected readonly int _seed;
        protected readonly int _machineCount;
        public MultiNodeSchedulerBase(int seed, int machineCount)
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
            var request = requestEnumerator.Current;

            while (partitionEnumerator.MoveNext())
            {
                while (requestEnumerator.Current.TimeStamp < partitionEnumerator.Current.start) // no partition yet
                {
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
                    request = requestEnumerator.Current;
                }

                // init all balances to the last seen syncronized value
                SplitBalances(balance, balances);

                while (requestEnumerator.Current.TimeStamp < partitionEnumerator.Current.start + partitionEnumerator.Current.duration) // in partition
                {
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
                    request = requestEnumerator.Current;
                }
                // merge the balances back to the total one
                balance = CombineBalances(balance, balances);
            }
            while (requestEnumerator.MoveNext())
            {
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
                request = requestEnumerator.Current;
            }

        }
        protected abstract decimal CombineBalances(decimal balance, decimal[] balances);

        protected abstract void SplitBalances(decimal balance, decimal[] balances);
    }
}