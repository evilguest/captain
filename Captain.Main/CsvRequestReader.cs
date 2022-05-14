using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Captain.Core;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace Captain
{
    public class CsvRequestReader : IRequestReader
    {
        protected class AlphaBankRequest
        {
            //[Name("Тип счёта")]
            //public string AccountType { get; set; }
            [Name("Дата операции")]
            public DateTimeOffset Date{get;set;}
            //[Name("Номер счета")]
            //public string AccountNo { get; set; }
            [Name("Референс проводки")]
            public string ReferenceNo { get; set; }
            [Name("Описание операции")]
            public string Comment { get; set; }
            [Name("Приход")]
            public decimal Charge { get; set; }
            [Name("Расход")]
            public decimal Withdrawal { get; set; }
            //[Name("Валюта")]
            //public string Currency { get; set; }
        }
        public IEnumerable<TransferRequest> Read(string filePath, string language)
        {
            return from r in new CsvReader(new StreamReader(filePath), CultureInfo.GetCultureInfoByIetfLanguageTag(language)).GetRecords<AlphaBankRequest>()
                   select new TransferRequest() { Id = GetUniqueId(r.ReferenceNo), TimeStamp = r.Date, Amount = r.Charge - r.Withdrawal };
        }

        private string GetUniqueId(string id) => id switch
        {
            "HOLD" or "PML0628AJLQ9C001" or "CRD_" => id + (++_count).ToString(),
            null or "" => (++_count).ToString(),
            _ => id,
        };

        private int _count = 0;
    }
}
