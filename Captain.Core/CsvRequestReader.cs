using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Captain.Core
{
    public class CsvRequestReader : IRequestReader
    {
        private class AlphaBankRequest
        {

        }
        public IEnumerable<TransferRequest> Read(string filePath) 
            => new CsvReader(new StreamReader(filePath), CultureInfo.InvariantCulture).GetRecords<TransferRequest>();
    }
}
