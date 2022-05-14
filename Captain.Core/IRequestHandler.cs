using Captain.Core;


namespace Captain
{
    public interface IRequestHandler
    {
        TransferResult ProcessRequest(TransferRequest request);
        void StartPartition();
        void FinishPartition();
    }
}