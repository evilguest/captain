using System;

namespace Captain.Core
{
    public record TransferResult(string Id, DateTimeOffset TimeStamp, decimal Amount, bool Confirmed) : TransferRequest(Id, TimeStamp, Amount)
    {
        //public TransferResult(string id) => Id = id;
        public TransferResult(TransferRequest r, bool confirmed) : this(r.Id, r.TimeStamp, r.Amount, confirmed) { }
    }
}
