using Captain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Captain
{
    public abstract class MultiNodeSchedulerBase : TransactionScheduler
    {
        protected readonly int _seed;
        protected readonly int _machineCount;
        public MultiNodeSchedulerBase(int seed, int machineCount)
        {
            _seed = seed;
            _machineCount = (machineCount > 0) 
                ? machineCount 
                : throw new ArgumentOutOfRangeException(nameof(machineCount), _machineCount, "Machine count must be above zero");
        }

        public virtual TransferResult ProcessNormal(ref decimal balance, TransferRequest request)
        {
            var confirmed = (balance + request.Amount >= 0);
            if (confirmed)
                balance += request.Amount;

            return new TransferResult(request.Id)
            {
                TimeStamp = request.TimeStamp,
                Amount = request.Amount,
                Balance = balance,
                Confirmed = confirmed,
            };
        }

        public virtual TransferResult ProcessSeparated(int machine, decimal[] balances, TransferRequest request)
        {
            var confirmed = (balances[machine] + request.Amount >= 0);
            if (confirmed)
                balances[machine] += request.Amount;

            return new TransferResult(request.Id)
            {
                TimeStamp = request.TimeStamp,
                Amount = request.Amount,
                Balance = balances[machine],
                Confirmed = confirmed,
            };

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
                while (request.TimeStamp < partitionEnumerator.Current.start) // no partition yet
                {
                    yield return ProcessNormal(ref balance, request);
                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                    request = requestEnumerator.Current;
                }

                // distribute the balances across the nodes according to the sync policy
                DistributeBalances(balance, balances);

                while (request.TimeStamp < partitionEnumerator.Current.finish()) // in partition
                {
                    // randomly choose the target host
                    var machine = r.Next(_machineCount);

                    yield return ProcessSeparated(machine, balances, request);

                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                    request = requestEnumerator.Current;
                }
                // merge the balances back to the total one
                balance = CollectBalances(balance, balances);
            }

            while (requestEnumerator.MoveNext())
            {
                yield return ProcessNormal(ref balance, request);

                if (!requestEnumerator.MoveNext())
                    yield break; // done
                request = requestEnumerator.Current;
            }

        }
        public abstract decimal CollectBalances(decimal balance, decimal[] balances);

        public abstract void DistributeBalances(decimal balance, decimal[] balances);
    }
}