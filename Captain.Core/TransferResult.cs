using System;

namespace Captain.Core
{
    public record TransferResult(string Id, DateTimeOffset TimeStamp, decimal Amount, bool Approved)
    {
        public TransferResult(TransferRequest r, bool approved) : this(r.Id, r.TimeStamp, r.Amount, approved) { }
    }
}
