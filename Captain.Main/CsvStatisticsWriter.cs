using Captain.Core;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;

namespace Captain.Main
{
    internal class CsvStatisticsWriter : System.IDisposable, IStatisticsWriter
    {
        private CsvWriter _csvWriter;

        public CsvStatisticsWriter(string resultFileName)
        {
            _csvWriter = new CsvWriter(new StreamWriter(GetFileName(resultFileName)), new CsvConfiguration(CultureInfo.InvariantCulture));
            _csvWriter.WriteHeader<StatisticsItem>();
            _csvWriter.NextRecord();
        }

        private string GetFileName(string resultFileName) 
            => $"{Path.GetFileNameWithoutExtension(resultFileName)}{DateTime.Now:yyyy-MM-dd.HH.mm.ss}.csv";

        public void WriteResult(StatisticsItem item)
        {
            _csvWriter.WriteRecord(item);
            _csvWriter.NextRecord();
        }

            public void Dispose() => _csvWriter.Dispose();
    }
}