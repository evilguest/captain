using Captain.Core;

namespace Captain.Paxos
{
    public class QuorumScheduler : MultiNodeSchedulerBase
    {
        public QuorumScheduler(int seed, int machineCount) : base(seed, machineCount)
        {
        }

        public override decimal CollectBalances(decimal balance, decimal[] balances) => balance;

        public override void DistributeBalances(decimal balance, decimal[] balances) { }

        public override TransferResult ProcessSeparated(int machine, decimal[] balances, TransferRequest request)
        {
            return base.ProcessSeparated(machine, balances, request);
        }

    }
}