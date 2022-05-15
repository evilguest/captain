using Captain.Core;

namespace Captain
{
    public class MajorityHandler: QuorumHandler
    {
        public MajorityHandler(int seed, int machineCount) : base(seed, machineCount, machineCount / 2 + 1) { }
    }
}