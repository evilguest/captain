using System.Collections.Generic;

namespace Captain.Core
{
    public interface ITransactionScheduler
    {
        public string Name { get; }
        public abstract IEnumerable<TransferResult> Play(IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule, IRequestHandler handler);
    }


}
