using System.Collections.Generic;

namespace Captain.Core
{
    public interface ITransactionScheduler
    {
        public abstract IEnumerable<TransferResult> Play(decimal initialBalance, IEnumerable<TransferRequest> requests, PartitionSchedule partitionSchedule);
    }


}
