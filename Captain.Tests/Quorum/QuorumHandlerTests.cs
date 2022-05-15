using Captain.Core;
using System;
using System.Linq;
using Xunit;

namespace Captain.Tests.Quorum
{
    public record PartitionScheduleParameters(TimeSpan MTTF, TimeSpan MTTR, TimeSpan TTRDispersion, DateTimeOffset Start) : IPartitionScheduleParameters;
    public class QuorumHandlerTests
    {
        [Fact]
        public void TestConstructorValidation()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new QuorumHandler(42, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QuorumHandler(42, 32, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => new QuorumHandler(42, 15, 16));
        }
        [Theory]
        [InlineData(42, 1)]
        [InlineData(42, 3)]
        [InlineData(42, 10)]
        public void TestOneMachineQuorum(int seed, int machineCount)
        {
            var q = new QuorumHandler(seed, machineCount, 1);
            var rs = TestHelper.GetTopupThenWithdrawals(10, 42, 10, TimeSpan.FromSeconds(1)).ToList();
            var sts = Analyzer.SingleMachineProcess(rs).ToArray();
            var qts = Evaluator.Play(rs, new PartitionSchedule(new PartitionScheduleParameters(TimeSpan.FromHours(1), TimeSpan.FromHours(1), new TimeSpan(0), DateTime.Now), seed), q).ToArray();
            Assert.Equal(sts, qts);
        }

        [Fact]
        public void TestTwoThirdsQuorum()
        {
            var q = new QuorumHandler(42, 3, 2);
            var rs = TestHelper.GetTopupThenWithdrawals(10, 42, 10, TimeSpan.FromSeconds(1)).ToList();
            var sts = Analyzer.SingleMachineProcess(rs).ToArray();
            var qts = Evaluator.Play(rs, new PartitionSchedule(new PartitionScheduleParameters(TimeSpan.FromHours(1), TimeSpan.FromHours(1), new TimeSpan(0), DateTime.Now), 42), q).ToArray();
            Assert.Equal(sts, qts);
        }
    }
}
