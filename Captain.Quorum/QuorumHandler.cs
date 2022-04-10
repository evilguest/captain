using Captain.Core;

namespace Captain
{
    public class QuorumHandler: PessimisticHandler
    {
        private int _quorumMap;
        private QuorumHandler(int seed, int machineCount, decimal initialBalance) : base(seed, machineCount , initialBalance){
            if (machineCount > 30)
                throw new ArgumentException($"{nameof(machineCount)} higher than 30 is not supported", nameof(machineCount));
        }
        public static new QuorumHandler Create(int seed, int machineCount, decimal initialBalance) => new QuorumHandler(seed, machineCount, initialBalance);

        public override decimal CollectBalances(decimal balance, decimal[] balances) => balance;

        public override void DistributeBalances(decimal balance, decimal[] balances) { }
        public override void StartPartition()
        {
            base.StartPartition(); // actually - nothing much to do
            // now we need to build the partition map.
            // Idea: we generate a random nonnegative number that has only the _machineCount bits possibly set.
            // Since _machineCount does never exceed 30, we can safely calculate 2^machineCount which will not exceed 1073741824, representable as both int and uint.
            var quorumMap = _random.Next(1 << _machineCount);
            // now we check that the "majority" flag does indeed indicate the majority; and if not - we flip the bits:
            var majCount = CountOnes((uint)quorumMap);
            if (majCount <= _machineCount / 2) // ouch, we have minority marked with 1s
                quorumMap = ~quorumMap; // invert! 
#if DEBUG
            // clearing the top bits for better look
            quorumMap &= (1 << _machineCount) - 1;
#endif
            _quorumMap = quorumMap;
        }

        public bool IsNodeInQuorum(int machine) => (_quorumMap & (1 << machine)) != 0;

        /// <summary>
        /// Counts the set bits in the unsigned int number
        /// Copyright(c) Peter Wegner, CACM 3 (1960), p.322
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static int CountOnes(uint x) 
        {
            int c; // c accumulates the total bits set in v
            for (c = 0; x > 0; c++)
                x &= x - 1; // clear the least significant bit set
            return c;
        }

        public override TransferResult ProcessPartitioned(int machine, TransferRequest request) 
            => IsNodeInQuorum(machine) 
                ? base.ProcessNormal(machine, request) 
                : base.ProcessPartitioned(machine, request);

    }
}