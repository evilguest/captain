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
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(23798)]
        public void TestSingleMachineProcess(int count)
        {
            const decimal w = 10;
            const decimal b = 42;
            var t = DateTime.Now;
            var request = new TransferRequest()
            {
                Amount = -w,
                Id = "1",
                TimeStamp = t,
            };
            var rs = Enumerable.Repeat(request, count).Prepend(new() { Amount=b, Id="0", TimeStamp=t});

            var ts = Analyzer.SingleMachineProcess(rs).ToArray();
            Assert.Equal(count+1, ts.Count());
            //Assert.All(t, s => Assert.True(s.Balance >= 0));
            Assert.All(ts.Take(1+(int)(b / w)), t => Assert.True(t.Confirmed));
            Assert.All(ts.Skip(1+(int)(b / w)), t => Assert.False(t.Confirmed));
        }
        [Fact]
        public void TestConsistency1()
        {
            var t = DateTime.Now;
            var results = new TransferResult[] {
                new("1")
                {
                    Amount = 1,
                    TimeStamp = t,
                    Confirmed = true
                },
                new("2")
                {
                    Amount = -2,
                    TimeStamp = t.AddHours(1),
                    Confirmed = true
                },
                new("3")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = true
                }
            };
            var c = Analyzer.GetConsistency(results);
            Assert.Equal(0.5, c);
        }
        [Fact]
        public void TestConsistency2()
        {
            var t = DateTime.Now;
            var results = new TransferResult[] {
                new("1")
                {
                    Amount = 1,
                    TimeStamp = t,
                    Confirmed = true
                },
                new("2")
                {
                    Amount = -1,
                    TimeStamp = t.AddHours(1),
                    Confirmed = true
                },
                new("3")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = true
                }
            };
            var c = Analyzer.GetConsistency(results);
            Assert.Equal(1, c);
        }
        [Fact]
        public void TestConsistency3()
        {
            var t = DateTime.Now;
            var results = new TransferResult[] {
                new("1")
                {
                    Amount = 1,
                    TimeStamp = t,
                    Confirmed = true
                },
                new("2")
                {
                    Amount = -2,
                    TimeStamp = t.AddHours(1),
                    Confirmed = false
                },
                new("3")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = true
                }
            };
            var c = Analyzer.GetConsistency(results);
            Assert.Equal(1, c);
        }
            [Fact]
        public void TestUltimateAcceptance()
        {
            var request = new TransferRequest()
            {
                Amount = -1,
                Id = "1",
                TimeStamp = DateTime.Now,
            };
            var rs = Enumerable.Repeat(request, 42);
            var ts = Analyzer.UltimateAcceptanceProcess(rs);
            Assert.Equal(rs.Count(), ts.Count());
            Assert.All(ts, t => Assert.True(t.Confirmed));
        }

        [Fact]
        public void TestAvailability()
        {
            var t = DateTime.Now;
            var rs = new TransferResult[] {
                new("1")
                {
                    Amount = 1,
                    TimeStamp = t,
                    Confirmed = true
                },
                new("2")
                {
                    Amount = -2,
                    TimeStamp = t.AddHours(1),
                    Confirmed = false
                },
                new("3")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = true
                },
                new("4")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(3),
                    Confirmed = false
                }

            };
            var rs1 = new TransferResult[] {
                new("1")
                {
                    Amount = 1,
                    TimeStamp = t,
                    Confirmed = true
                },
                new("1")
                {
                    Amount = -2,
                    TimeStamp = t.AddHours(1),
                    Confirmed = false
                },
                new("3")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = true
                },
                new("4")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = false
                }
            };
            Assert.Throws<InvalidOperationException>(() => Analyzer.GetAvailability(rs1, rs));
            var rs2 = new TransferResult[] {
                new("1")
                {
                    Amount = 1,
                    TimeStamp = t,
                    Confirmed = true
                },
                new("2")
                {
                    Amount = -2,
                    TimeStamp = t.AddHours(1),
                    Confirmed = true
                },
                new("3")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = false
                },
                new("4")
                {
                    Amount = 1,
                    TimeStamp = t.AddHours(2),
                    Confirmed = false
                }
            };
            Assert.Equal(0.5, Analyzer.GetAvailability(rs2, rs));

        }
        [Fact]
        public void TestNetworkAvailability()
        {
            var start = DateTime.Now;
            var ps = new Partition[] {
                new (start.AddHours(1), TimeSpan.FromHours(12)),
                new (start.AddHours(25), TimeSpan.FromHours(24)),
                new (start.AddDays(5), TimeSpan.FromDays(100))
            };
            Assert.Equal(0.5, Analyzer.GetNetworkAvailability(ps, start, start.AddDays(3)));
        }
    }
}
