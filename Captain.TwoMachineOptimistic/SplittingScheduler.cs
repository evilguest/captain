using Captain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public class SplittingScheduler: MultiNodeSchedulerBase
    {
        public SplittingScheduler(int seed, int machineCount): base(seed, machineCount)
        {
        }

        protected override decimal CombineBalances(decimal balance, decimal[] balances) => balances.Sum();

        protected override void SplitBalances(decimal balance, decimal[] balances)
        {
            foreach (ref var b in balances.AsSpan())
                b = balance/balances.Length; // TODO: round this up to two digits after the point
        }
    }
}
