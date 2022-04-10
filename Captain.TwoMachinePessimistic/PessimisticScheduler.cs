using Captain.Core;
using System;
using System.Collections.Generic;

namespace Captain
{
    public class PessimisticScheduler : MultiNodeSchedulerBase
    {
        public PessimisticScheduler(int seed, int machineCount) : base(seed, machineCount)
        {
        }

        public override void DistributeBalances(decimal balance, decimal[] balances) => balances[0] = balance;
        public override decimal CollectBalances(decimal balance, decimal[] balances) => balances[0];
        public override TransferResult ProcessSeparated(int machine, decimal[] balances, TransferRequest request)
        {
            return new TransferResult(request.Id)
            {
                TimeStamp = request.TimeStamp,
                Amount = request.Amount,
                Balance = balances[0],
                Confirmed = false,
            };
        }
    }
}
