using Captain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Captain
{
    using HandlerFactory = System.Func<int, int, decimal, ITransactionHandler>;
    public class TransactionScheduler: ITransactionScheduler
    {
        protected readonly int _seed;
        protected readonly int _machineCount;
        protected readonly HandlerFactory _handlerFactory;
        public TransactionScheduler(int seed, int machineCount, HandlerFactory handlerFactory)
        {
            _seed = seed;
            _machineCount = (machineCount > 0) 
                ? machineCount 
                : throw new ArgumentOutOfRangeException(nameof(machineCount), _machineCount, "Machine count must be above zero");
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory), "Handler factory is required");
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
        public IEnumerable<TransferResult> Play(decimal initialBalance, IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule)
        {
            if (requests is null)
                throw new ArgumentNullException(nameof(requests));

            if (partitionSchedule is null)
                throw new ArgumentNullException(nameof(partitionSchedule));

            var h = _handlerFactory(_seed, _machineCount, initialBalance);

            //var balance = initialBalance;
            //var balances = new decimal[_machineCount];
            using var requestEnumerator = requests.GetEnumerator();
            using var partitionEnumerator = partitionSchedule.GetPartitions().GetEnumerator();
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

                while (request.TimeStamp < partitionEnumerator.Current.finish()) // in partition
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