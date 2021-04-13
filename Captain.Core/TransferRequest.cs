using System;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace Captain.Core
{
    public class TransferRequest
    {
        public DateTimeOffset TimeStamp { get; set; }
        public decimal Amount { get; set; }

    }
}
