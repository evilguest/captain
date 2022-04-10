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

        public override decimal CollectBalances(decimal balance, decimal[] balances) => balances.Sum();

        public override void DistributeBalances(decimal balance, decimal[] balances)
        {
            int cents = (int)(balance * 100);
            var share = cents / balances.Length;
            var rem = cents % balances.Length;
            for (int i = 0; i < rem; i++)
                balances[i] = ((decimal)(share + 1)) / 100;
            for (int i = rem; i < balances.Length; i++)
                balances[i] = ((decimal)share) / 100;
            // now we need to fix up the possible remainder of the cents rounding
            balances[0] += balance - ((decimal)cents)/100;
        }
    }
}
