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

        private static IEnumerable<TransferRequest> AddAttempts(IEnumerable<TransferRequest> requests)
        {
            var balance = 0m;
            var t = Analyzer.SingleMachineProcess(requests);
            var h2 = from rs in t select (R:rs, B: balance += rs.Approved ? rs.Amount : 0);
            var dups = 0;
            foreach(var h in h2)
            {
                yield return new TransferRequest(h.R.Id, h.R.TimeStamp, h.R.Amount);
                if (h.B + h.R.Amount < 0)
                {
                    yield return new TransferRequest(h.R.Id, h.R.TimeStamp.AddMinutes(1), h.R.Amount);
                    dups++;
                }
            }
            Console.WriteLine($"Added {dups} duplicate records.");
        }
        private static int RunOptionsAndReturnExitCode(Options opts)
        {
            var requests = (from r in new CsvRequestReader().Read(opts.RequestsFileName, opts.Language)
                           select r).Reverse().ToList();//.OrderBy(r => r.TimeStamp);
            Console.WriteLine($"Succesfully loaded file {opts.RequestsFileName}");
            Console.WriteLine($"{requests.Count()} entries found");
            Console.WriteLine($"First entry {requests.First().TimeStamp}, last entry {requests.Last().TimeStamp}. ");
            //HandleHistory(opts, requests);
            Console.WriteLine("Now we're going to add some more risky transactions");
            var history = AddAttempts(requests).ToList();
            HandleHistory(opts, history);
            return 0;
        }
        private static void HandleHistory(Options opts, IEnumerable<TransferRequest> history)
        {
            using (IStatisticsWriter sw = string.IsNullOrWhiteSpace(opts.ResultFileName)
                ? new NullWriter()
                : new CsvStatisticsWriter(opts.ResultFileName))
            {

                Console.WriteLine($"{history.Count(e => e.Amount < 0)} widthdrawals, {history.Count(e => e.Amount >= 0)} top-ups.");
                Console.WriteLine($"Operations balance {history.Sum(e => e.Amount)}");
                //Console.Read();
                Console.WriteLine("Checking the single-machine processor consistency...");
                var singleMachineHistory = history.SingleMachineProcess();
                Console.WriteLine($"Operations balance {singleMachineHistory.Sum(e => e.Amount)}; final balance {singleMachineHistory.Where(r => r.Approved).Sum(e => e.Amount)}");
                var lowestConsistencyHistory = history.UltimateAcceptanceProcess();
                Console.WriteLine($"Lowest consistency is {lowestConsistencyHistory.GetConsistency():p2}");

                //Console.Read();
                if (opts.Start == new DateTimeOffset())
                    opts.Start = history.First().TimeStamp;
                var g = new PartitionScheduleGenerator(opts, opts.Seed);
                //Console.WriteLine($"Running the pessimistic scheduler...");
                //var (c, a, na) = EstimateConsistencyAvailability("Pessimistic", () => new PessimisticHandler(opts.Seed, opts.NodeCount), history, g, opts.Iterations, sw);
                //Console.WriteLine($"Pessimistic  : C = {c:p2}, A = {a:p2}, P = {na:p2}");
                //Console.WriteLine($"Running the optimistic scheduler...");
                //(c, a, na) = EstimateConsistencyAvailability($"Optimistic ({opts.NodeCount})", () => new OptimisticHandler(opts.Seed, opts.NodeCount), history, g, opts.Iterations, sw);
                //Console.WriteLine($"Optimistic({opts.NodeCount}): C = {c:p2}, A = {a:p2}, P = {na:p2}");

                for (int i = 2; i <= opts.NodeCount; i++)
                {
                    Console.WriteLine($"Running the splitting scheduler...");
                    var (c, a, na) = EstimateConsistencyAvailability($"Splitting ({i})", () => new SplittingHandler(opts.Seed, i), history, g, opts.Iterations, sw);
                    Console.WriteLine($"Splitting({i}): C = {c:p2}, A = {a:p2}, P = {na:p2}");
                }

                for (int i = 3; i <= opts.NodeCount; i++)
                {
                    Console.WriteLine($"Running the majority scheduler...");
                    var (c, a, na) = EstimateConsistencyAvailability($"Majority ({i})", () => QuorumHandler.CreateMajorityHandler(opts.Seed, i), history, g, opts.Iterations, sw);
                    Console.WriteLine($"Majority ({i}): C = {c:p2}, A = {a:p2}, P = {na:p2}");
                }

                //for (int i = 1; i <= opts.NodeCount; i++)
                //{
                //    Console.WriteLine($"Running the quorum scheduler...");
                //    var (c, a, na) = EstimateConsistencyAvailability($"Quorum({i}/{opts.NodeCount})", () => new QuorumHandler(opts.Seed, opts.NodeCount, i), history, g, opts.Iterations, sw);
                //    Console.WriteLine($"Quorum ({i}/{opts.NodeCount}): C = {c:p2}, A = {a:p2}, P = {na:p2}");
                //}

            }

        }
    }
}
