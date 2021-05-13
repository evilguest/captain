using Captain.Core;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Main
{
    class Program
    {
        public class Options : IPartitionScheduleParameters
        {
            [Option('b', "balance", Required = false, HelpText = "Sets an initial balance of the account")]
            public decimal InitialBalance { get; set; }
            [Option('f', "file", Required = true)]
            public string RequestsFileName { get; set; }

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

            public int Seed { get; set; }
            #endregion
        }
        static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args).MapResult(
                opts => RunOptionsAndReturnExitCode(opts), //in case parser sucess
                errs => HandleParseError(errs)); //in  case parser fail
        }

        private static int HandleParseError(IEnumerable<Error> errs)
        {
            var result = -2;
            Console.WriteLine("errors {0}", errs.Count());
            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                result = -1;
            Console.WriteLine("Exit code {0}", result);
            return result;
        }

        private static int RunOptionsAndReturnExitCode(Options opts)
        {
            var history = (from r in new CsvRequestReader().Read(opts.RequestsFileName)
                           select r).Reverse().OrderBy(r=>r.TimeStamp).ToList();
            Console.WriteLine($"Succesfully loaded file {opts.RequestsFileName}");
            Console.WriteLine($"{history.Count} entries found");
            Console.WriteLine($"First entry {history.First().TimeStamp}, last entry {history.Last().TimeStamp}. ");
            Console.WriteLine($"Operations balance {history.Sum(e => e.Amount)}; final balance {opts.InitialBalance+history.Sum(e => e.Amount)}");
            Console.Read();
            Console.WriteLine("Checking the single-machine processor consistency...");
            var res = history.SingleMachineProcess(opts.InitialBalance);
            Console.WriteLine($"Operations balance {res.Sum(e => e.Amount)}; final balance {opts.InitialBalance + res.Where(r => r.Confirmed).Sum(e => e.Amount)}");
            Console.Read();
            if (opts.Start == new DateTimeOffset())
                opts.Start = history[0].TimeStamp;
            var g = new PartitionScheduleGenerator(opts);
            var e = new Evaluator();
            Console.WriteLine($"Running the pessimistic scheduler...");
            var (c, a, na) = e.EstimateConsistencyAvailability(opts.InitialBalance, new PessimisticScheduler(), history, g, opts.Iterations);
            Console.WriteLine($"Pessimistic  : C = {c:p2}, A = {a:p2}, NA = {na:p2}");
            Console.WriteLine($"Running the optimistic scheduler...");
            (c, a, na) = e.EstimateConsistencyAvailability(opts.InitialBalance, new OptimisticScheduler(opts.Seed, 2), history, g, opts.Iterations);
            Console.WriteLine($"Optimistic(20): C = {c:p2}, A = {a:p2}, NA = {na:p2}");
            Console.WriteLine($"Running the splitting scheduler...");
            (c, a, na) = e.EstimateConsistencyAvailability(opts.InitialBalance, new SplittingScheduler(opts.Seed, 2), history, g, opts.Iterations);
            Console.WriteLine($"Splitting(2): C = {c:p2}, A = {a:p2}, NA = {na:p2}");
            return 0;
        }
    }
}
