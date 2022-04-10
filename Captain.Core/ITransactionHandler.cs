using Captain.Core;


namespace Captain
{
    public interface ITransactionHandler
    {
        TransferResult ProcessRequest(TransferRequest request);
        void StartPartition();
        void FinishPartition();
    }
}