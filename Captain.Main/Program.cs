using Captain.Core;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Captain.Core.Evaluator;

namespace Captain.Main
{
    public partial class Program
    {
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
            var history = (from r in new CsvRequestReader().Read(opts.RequestsFileName, opts.Language)
                           select r).Reverse().OrderBy(r => r.TimeStamp).ToList();

            using (IStatisticsWriter sw = string.IsNullOrWhiteSpace(opts.ResultFileName)
                ? new NullWriter()
                : new CsvStatisticsWriter(opts.ResultFileName))
            {

                Console.WriteLine($"Succesfully loaded file {opts.RequestsFileName}");
                Console.WriteLine($"{history.Count} entries found");
                Console.WriteLine($"First entry {history.First().TimeStamp}, last entry {history.Last().TimeStamp}. ");
                Console.WriteLine($"{history.Count(e => e.Amount<0)} widthdrawals, {history.Count(e=>e.Amount>=0)} top-ups.");
                Console.WriteLine($"Operations balance {history.Sum(e => e.Amount)}");
                //Console.Read();
                Console.WriteLine("Checking the single-machine processor consistency...");
                var singleMachineHistory = history.SingleMachineProcess();
                Console.WriteLine($"Operations balance {singleMachineHistory.Sum(e => e.Amount)}; final balance {singleMachineHistory.Where(r => r.Confirmed).Sum(e => e.Amount)}");
                var lowestConsistencyHistory = history.UltimateAcceptanceProcess();
                Console.WriteLine($"Lowest consistency is {lowestConsistencyHistory.GetConsistency():p2}"); 

                //Console.Read();
                if (opts.Start == new DateTimeOffset())
                    opts.Start = history[0].TimeStamp;
                var g = new PartitionScheduleGenerator(opts);
                Console.WriteLine($"Running the pessimistic scheduler...");
                var pessimistic = new TransactionScheduler(opts.Seed, opts.NodeCount, PessimisticHandler.Create);
                var (c, a, na) = EstimateConsistencyAvailability(pessimistic, history, g, opts.Iterations, sw);
                Console.WriteLine($"Pessimistic  : C = {c:p2}, A = {a:p2}, NA = {na:p2}");
                Console.WriteLine($"Running the optimistic scheduler...");
                (c, a, na) = EstimateConsistencyAvailability(new TransactionScheduler(opts.Seed, opts.NodeCount, OptimisticHandler.Create), history, g, opts.Iterations, sw);
                Console.WriteLine($"Optimistic({opts.NodeCount}): C = {c:p2}, A = {a:p2}, NA = {na:p2}");

                Console.WriteLine($"Running the splitting scheduler...");
                (c, a, na) = EstimateConsistencyAvailability(new TransactionScheduler(opts.Seed, opts.NodeCount, SplittingHandler.Create), history, g, opts.Iterations, sw);
                Console.WriteLine($"Splitting({opts.NodeCount}): C = {c:p2}, A = {a:p2}, NA = {na:p2}");

                Console.WriteLine($"Running the quorum scheduler...");
                (c, a, na) = EstimateConsistencyAvailability(new TransactionScheduler(opts.Seed, opts.NodeCount, MajorityHandler.Create), history, g, opts.Iterations, sw);
                Console.WriteLine($"Quorum({opts.NodeCount}): C = {c:p2}, A = {a:p2}, NA = {na:p2}");

                return 0;
            }
        }
    }
}
