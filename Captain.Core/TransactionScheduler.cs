using System.Collections.Generic;

namespace Captain.Core
{
    public abstract class TransactionScheduler
    {
        public abstract IEnumerable<TransferResult> Play(decimal initialBalance, IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule);
    }
}
