using Captain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public class OptimisticHandler: RequestHandlerBase
    {
        public OptimisticHandler(int seed, int machineCount): base(seed, machineCount){}

        protected override decimal CollectBalances(decimal balance, decimal[] balances) => balances.CollectDeltas(balance);

        protected override void DistributeBalances(decimal balance, decimal[] balances) => balances.Equalize(balance);
    }
}
