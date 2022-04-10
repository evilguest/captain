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
            TransferRequest request = new TransferRequest()
            {
                Amount = -w,
                Id = "1",
                TimeStamp = DateTime.Now,
            };
            var r = Enumerable.Repeat(request, count);
            
            var t = Analyzer.SingleMachineProcess(r, b);
            Assert.Equal(count, t.Count());
            Assert.All(t, s => Assert.True(s.Balance >= 0));
            Assert.All(t.Take((int)(b / w)), s => Assert.True(s.Confirmed));
            Assert.All(t.Skip((int)(b / w)), s => Assert.False(s.Confirmed));
        }
    }
}
