using System;
using System.Linq;
using Xunit;

namespace Captain.Tests
{
    public class TestMoneyHelper
    {
        [Theory]
        [InlineData(1, 1.00)]
        [InlineData(2, 1.00)]
        [InlineData(3, 17.00)]
        [InlineData(7, 17.89)]
        [InlineData(7, 17.8972)]
        public void TestDistribute(int machineCount, decimal balance)
        {
//            var s = SplittingHandler.Create(42, machineCount);
            var balances = new decimal[machineCount];
            balances.Distribute(balance);
            Assert.Equal(balance, balances.Sum());
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(7)]
        [InlineData(8)]
        public void TestCollect(int machineCount)
        {
            var balances = new decimal[machineCount];
            var r = new Random(42);
            foreach(ref var b in balances.AsSpan())
                b = ((decimal)r.Next())/100;
            Assert.Equal(balances.Sum(), balances.Collect());
        }
    }
}
