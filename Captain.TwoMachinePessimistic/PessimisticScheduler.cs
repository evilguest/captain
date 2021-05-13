﻿using Captain.Core;
using System;
using System.Collections.Generic;

namespace Captain
{
    public class PessimisticScheduler : TransactionScheduler
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
                var request = requestEnumerator.Current;
                while (request.TimeStamp < partitionEnumerator.Current.start) // no partition yet
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
                while (request.TimeStamp < partitionEnumerator.Current.start + partitionEnumerator.Current.duration) // in partition
                {
                    yield return new TransferResult(request.Id)
                    {
                        TimeStamp = request.TimeStamp,
                        Amount = request.Amount,
                        Balance = balance,
                        Confirmed = false,
                    };
                    if (!requestEnumerator.MoveNext())
                        yield break; // done
                    request = requestEnumerator.Current;
                }
            }
            while(requestEnumerator.MoveNext()) // past the final partition
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
            }
        }

    }
}
