using Captain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public class SplittingHandler: RequestHandlerBase
    {
        private SplittingHandler(int seed, int machineCount): base(seed, machineCount) {}

        public static SplittingHandler Create(int seed, int machineCount) => new SplittingHandler(seed, machineCount);

        protected override decimal CollectBalances(decimal balance, decimal[] balances) 
            => balances.Sum();

        protected override void DistributeBalances(decimal balance, decimal[] balances) => balances.Distribute(balance);
    }
}
