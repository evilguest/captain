using Captain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public class OptimisticHandler: TransactionHandlerBase
    {
        private OptimisticHandler(int seed, int machineCount, decimal initialBalance): base(seed, machineCount, initialBalance){}
        public static OptimisticHandler Create(int seed, int machineCount, decimal initialBalance) => new OptimisticHandler(seed, machineCount, initialBalance);

        public override decimal CollectBalances(decimal balance, decimal[] balances) => balance + balances.Sum() - balance * balances.Length;

        public override void DistributeBalances(decimal balance, decimal[] balances)
        {
            foreach (ref var b in balances.AsSpan())
                b = balance; 
        }
    }
}
