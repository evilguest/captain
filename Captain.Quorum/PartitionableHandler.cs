namespace Captain
{
    public abstract class PartitionableHandler : RequestHandlerBase
    {
        public enum ClusterPart : byte { A, B };
        private int _partitionMap;
        public PartitionableHandler(int seed, int machineCount) : base(seed, machineCount)
        {
            if (machineCount > 30)
                throw new ArgumentOutOfRangeException(nameof(machineCount), machineCount, $"{nameof(machineCount)} higher than 30 is not supported");
        }

        public override void StartPartition()
        {
            base.StartPartition();

            // now we need to build the partition map.
            // Idea: we generate a random nonnegative number that has only the _machineCount bits possibly set.
            // Since _machineCount does never exceed 30, we can safely calculate 2^machineCount which will not exceed 1073741824, representable as both int and uint.

            while (_partitionMap == 0) // we need to make sure there are two parts in the cluster!
                _partitionMap = _random.Next(1 << _machineCount);
        }

        public override void FinishPartition()
        {
            base.FinishPartition();
            _partitionMap = 0;
        }
        public ClusterPart NodePart(int machine)
            => (_partitionMap & (1 << machine)) == 0 ? ClusterPart.A : ClusterPart.B;

        protected int CountSimilarMachines(int machine) => NodePart(machine) switch
        {
            ClusterPart.A => _machineCount - CountOnes((uint)_partitionMap),
            ClusterPart.B => CountOnes((uint)_partitionMap),
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
    }
}