using Captain.Core;
using CommandLine;
using System;

namespace Captain.Main
{
    public partial class Program
    {
        public class Options : IPartitionScheduleParameters
        {
            //[Option('b', "balance", Required = false, HelpText = "Sets an initial balance of the account")]
            //public decimal InitialBalance { get; set; }
            [Option('f', "file", Required = true)]
            public string RequestsFileName { get; set; }
            [Option('o', "output", Required = false)]
            public string ResultFileName { get; set; }

            [Option(Default = 100)]
            public int Iterations { get; set; }
            #region Partition Schedule Parameters
            [Option]
            public TimeSpan MTTF { get; set; }
            [Option]
            public TimeSpan MTTR { get; set; }
            [Option]
            public TimeSpan TTRDispersion { get; set; }

            [Option]
            public DateTimeOffset Start { get; set; }
            [Option]
            public int Seed { get; set; }
            #endregion
            [Option('l', "language", Default ="ru-RU")]
            public string Language { get; set; }
            [Option('n', "nodes", Default = 2)]
            public int NodeCount { get; set; } = 2;

        }
    }
}
