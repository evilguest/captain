using System;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace Captain.Core
{
    public record TransferRequest(string Id, DateTimeOffset TimeStamp, decimal Amount);
}
