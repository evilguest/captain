using Captain.Core;
using System;
using System.Collections.Generic;

namespace Captain
{
    public class PessimisticHandler : RequestHandlerBase
    {

        public PessimisticHandler(int seed, int machineCount) : base(seed, machineCount)
        {
        }

        public static PessimisticHandler Create(int seed, int machineCount) => new PessimisticHandler(seed, machineCount);

        protected override void DistributeBalances(decimal balance, decimal[] balances) {}
        protected override decimal CollectBalances(decimal balance, decimal[] balances) => balance;
        /// <summary>
        /// Handles the request when nodes are partitioned.
        /// This version always rejects the request while nodes can't talk to each other
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override TransferResult ProcessPartitioned(int machine, TransferRequest request) => request.Reject();
    }
}
