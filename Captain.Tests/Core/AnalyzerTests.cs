using Captain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Captain.Tests.Core
{
    public class AnalyzerTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(23798)]
        public void TestSingleMachineProcess(int count)
        {
            var rs = TestHelper.GetTopupThenWithdrawals(count, 42m, 10m, TimeSpan.FromMinutes(10));

            var ts = Analyzer.SingleMachineProcess(rs).ToArray();
            Assert.Equal(count, ts.Count());
            //Assert.All(t, s => Assert.True(s.Balance >= 0));
            Assert.All(ts.Take(1 + (int)((decimal)42 / 10)), t => Assert.True(t.Approved));
            Assert.All(ts.Skip(1 + (int)((decimal)42 / 10)), t => Assert.False(t.Approved));
        }


        [Fact]
        public void TestConsistency1()
        {
            var t = DateTime.Now;
            var results = new TransferResult[] {
                new("1", t, 1, true),
                new("2", t.AddHours(1), -2, true),
                new("3", t.AddHours(2), 1, true)
            };
            var c = Analyzer.GetConsistency(results);
            Assert.Equal(0.5, c);
        }
        [Fact]
        public void TestConsistency2()
        {
            var t = DateTime.Now;
            var results = new TransferResult[] {
                new("1", t, 1, true),
                new("2", t.AddHours(1), -1, true),
                new("3", t.AddHours(2), 1, true)
            };
      
            var c = Analyzer.GetConsistency(results);
            Assert.Equal(1, c);
        }
        [Fact]
        public void TestConsistency3()
        {
            var t = DateTime.Now;
            var results = new TransferResult[] {
                new("1", t, 1, true),
                new("2", t.AddHours(1), -2, false),
                new("3", t.AddHours(2), 1, true)
            };
            var c = Analyzer.GetConsistency(results);
            Assert.Equal(1, c);
        }
            [Fact]
        public void TestUltimateAcceptance()
        {
            var request = new TransferRequest("1", DateTimeOffset.Now, -1);
            var rs = Enumerable.Repeat(request, 42);
            var ts = Analyzer.UltimateAcceptanceProcess(rs);
            Assert.Equal(rs.Count(), ts.Count());
            Assert.All(ts, t => Assert.True(t.Approved));
        }

        [Fact]
        public void TestAvailability()
        {
            var t = DateTime.Now;
            var rs = new TransferResult[] {
                new("1", t, 1, true),
                new("2", t.AddHours(1), -2, false),
                new("3", t.AddHours(2), 1, true),
                new("4", t.AddHours(3), 1, false)
            };
            var rs1 = new TransferResult[] {
                new("1", t, 1, true),
                new("1", t.AddHours(1), -2, false),
                new("3", t.AddHours(2), 1, true),
                new("4", t.AddHours(2), 1, false)
            };
            Assert.Throws<InvalidOperationException>(() => Analyzer.GetAvailability(rs1, rs));
            var rs2 = new TransferResult[] {
                new("1", t, 1, true),
                new("2", t.AddHours(1), -2, false),
                new("3", t.AddHours(2), 1, false),
                new("4", t.AddHours(2), 1, false)
            };
            Assert.Equal(0.5, Analyzer.GetAvailability(rs2, rs));

        }
        [Fact]
        public void TestNetworkAvailability()
        {
            var start = DateTime.Now;
            var ps = new Partition[] {
                new (start.AddHours(1), TimeSpan.FromHours(12)),
                new (start.AddDays(2), TimeSpan.FromHours(36)),
                new (start.AddDays(5), TimeSpan.FromDays(100))
            };
            Assert.Equal(0.5, Analyzer.GetNetworkAvailability(ps, start, start.AddDays(3)));
        }
    }
}
