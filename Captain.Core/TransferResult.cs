namespace Captain.Core
{
    public class TransferResult : TransferRequest
    {
        public bool Confirmed { get; set; }
        public decimal Balance { get; set; }
    }
}
