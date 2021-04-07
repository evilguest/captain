using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Captain.Core
{
    public class CsvRequestReader : IRequestReader
    {
        public IEnumerable<TransferRequest> Read(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<TransferRequest>();
        }
    }
}
