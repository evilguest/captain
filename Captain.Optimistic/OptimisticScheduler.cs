using Captain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public class OptimisticScheduler: MultiNodeSchedulerBase
    {
        public OptimisticScheduler(int seed, int machineCount): base(seed, machineCount){}

        public override decimal CollectBalances(decimal balance, decimal[] balances) => balance + balances.Sum() - balance * balances.Length;

        public override void DistributeBalances(decimal balance, decimal[] balances)
        {
            foreach (ref var b in balances.AsSpan())
                b = balance; 
        }
    }
}
