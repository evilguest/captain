using Captain.Core;
using System;
using System.Collections.Generic;

namespace Captain
{
    public class TwoMachinePessimisticScheduler : TransactionScheduler
    {
        public override IEnumerable<TransferResult> Play(decimal initialBalance, IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule)
        {
            if (requests is null)
                throw new ArgumentNullException(nameof(requests));

            if (partitionSchedule is null)
                throw new ArgumentNullException(nameof(partitionSchedule));

            var balance = initialBalance;
            using var requestEnumerator = requests.GetEnumerator();
            using var partitionEnumerator = partitionSchedule.GetPartitions().GetEnumerator();
            if (!requestEnumerator.MoveNext())
                yield break; // nothing to return

            while (partitionEnumerator.MoveNext())
            {
                while (requestEnumerator.Current.TimeStamp < partitionEnumerator.Current.start) // no partition yet
                {
                    var confirmed = (balance + requestEnumerator.Current.Amount >= 0);
                    if (confirmed)
                        balance += requestEnumerator.Current.Amount;

                    yield return new TransferResult(requestEnumerator.Current.Id)
                    {
                        TimeStamp = requestEnumerator.Current.TimeStamp,
                        Amount = requestEnumerator.Current.Amount,
                        Balance = balance,
                        Confirmed = confirmed,
                    };
                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                }
                while (requestEnumerator.Current.TimeStamp < partitionEnumerator.Current.start + partitionEnumerator.Current.duration) // in partition
                {
                    yield return new TransferResult(requestEnumerator.Current.Id)
                    {
                        TimeStamp = requestEnumerator.Current.TimeStamp,
                        Amount = requestEnumerator.Current.Amount,
                        Balance = balance,
                        Confirmed = false,
                    };
                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                }
            }
            while(requestEnumerator.MoveNext())
            {

            }
        }

    }
}
