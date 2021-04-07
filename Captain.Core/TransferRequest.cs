using System;

namespace Captain.Core
{
    public class TransferRequest
    {
        public DateTimeOffset TimeStamp { get; set; }
        public decimal Amount { get; set; }

    }
}
