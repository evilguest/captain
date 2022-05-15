using Captain.Core;

namespace Captain
{
    public class MajorityHandler: QuorumHandler
    {
        public MajorityHandler(int seed, int machineCount) : base(seed, machineCount, machineCount / 2 + 1) { }
        public override TransferResult ProcessPartitioned(int machine, TransferRequest request) 
            => IsNodeInQuorum(machine) 
                ? base.ProcessNormal(machine, request) 
                : base.ProcessPartitioned(machine, request);
    }
}