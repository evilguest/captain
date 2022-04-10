using Captain.Core;
using System;
using System.Collections.Generic;

namespace Captain
{
    public class PessimisticHandler : TransactionHandlerBase
    {

        public PessimisticHandler(int seed, int machineCount, decimal initialBalance) : base(seed, machineCount, initialBalance)
        {
        }

        public static PessimisticHandler Create(int seed, int machineCount, decimal initialBalance) => new PessimisticHandler(seed, machineCount, initialBalance);

        public override void DistributeBalances(decimal balance, decimal[] balances) => balances[0] = balance;
        public override decimal CollectBalances(decimal balance, decimal[] balances) => balances[0];
        /// <summary>
        /// Handles the request when nodes are partitioned.
        /// This version always rejects the request while nodes can't talk to each other
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override TransferResult ProcessPartitioned(int machine, TransferRequest request)
        {
            return new TransferResult(request.Id)
            {
                TimeStamp = request.TimeStamp,
                Amount = request.Amount,
                Balance = _nodeBalances[0],
                Confirmed = false,
            };
        }
    }
}
