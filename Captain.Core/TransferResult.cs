namespace Captain.Core
{
    public class TransferResult : TransferRequest
    {
        public TransferResult(string id) => Id = id;
        public bool Confirmed { get; set; }
        public decimal Balance { get; set; }
    }
}
