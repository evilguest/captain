using Captain.Core;
namespace Captain
{
    public class QuorumHandler : RequestHandlerBase
    {
        private int _quorumMap;
        private readonly int _quorumSize;
        private readonly decimal[] _partBalances = new decimal[2];

        public static QuorumHandler Create(int seed, int machineCount, int quorumSize) => new QuorumHandler(seed, machineCount, quorumSize);
        public static QuorumHandler CreateMajorityHandler(int seed, int machineCount) => new QuorumHandler(seed, machineCount, machineCount / 2 + 1);
        private QuorumHandler(int seed, int machineCount, int quorumSize) : base(seed, machineCount)
        {
            if (machineCount > 30)
                throw new ArgumentOutOfRangeException(nameof(machineCount), machineCount, $"{nameof(machineCount)} higher than 30 is not supported");
            if (quorumSize < 1  || quorumSize > machineCount)
                throw new ArgumentOutOfRangeException(nameof(quorumSize), quorumSize, $"{nameof(quorumSize)} must be in the range from 1 to {nameof(machineCount)} ({machineCount}), inclusive");

            _quorumSize = quorumSize;
        }

        protected override decimal CollectBalances(decimal balance, decimal[] balances) => balances.CollectDeltas(balance);

        protected override void DistributeBalances(decimal balance, decimal[] balances) => balances.Equalize(balance);

        public override void StartPartition()
        {
            base.StartPartition();

            // now we need to build the partition map.
            // Idea: we generate a random nonnegative number that has only the _machineCount bits possibly set.
            // Since _machineCount does never exceed 30, we can safely calculate 2^machineCount which will not exceed 1073741824, representable as both int and uint.

            _quorumMap = 1 + _random.Next((1 << _machineCount) - 1); // generating an uniformly distributed number in the range [1..2^N-1]
            _partBalances.Equalize(_balance); // ensure both parts of the cluster get the same balance
        }
        public override void FinishPartition()
        {
            var b = _balance;
            base.FinishPartition();
            _balance = CollectBalances(b, _partBalances);
            _quorumMap = 0;
        }
        // we need to calculate whether the node belongs to a large enough group.
        public bool IsNodeInQuorum(int machine) => CountSimilarMachines(machine) >= _quorumSize;

        public enum ClusterPart:byte {A, B};
        public ClusterPart NodePart(int machine)
            => (_quorumMap & (1 << machine)) == 0 ? ClusterPart.A : ClusterPart.B;

        private int CountSimilarMachines(int machine) => NodePart(machine) switch
        {
            ClusterPart.A => _machineCount - CountOnes((uint)_quorumMap),
            ClusterPart.B => CountOnes((uint)_quorumMap),
            _ => throw new NotImplementedException(),
        };



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
                ? ProcessPartitionedInQuorum(NodePart(machine), request)
                : request.Reject();

        private TransferResult ProcessPartitionedInQuorum(ClusterPart clusterPart, TransferRequest request)
            => new(request, _partBalances[(int)clusterPart].CanAdd(request.Amount));
        
    }
}