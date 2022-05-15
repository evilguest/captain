using Captain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public class SplittingHandler: RequestHandlerBase
    {
        public SplittingHandler(int seed, int machineCount): base(seed, machineCount) {}
        protected override decimal CollectBalances(decimal balance, decimal[] balances) 
            => balances.Sum();
        protected override void DistributeBalances(decimal balance, decimal[] balances) => balances.Distribute(balance);
    }
}
