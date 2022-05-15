using System;
using System.Collections.Generic;

namespace Captain.Core
{
    public interface IPartitionScheduleParameters
    {
        public TimeSpan MTTF { get;  }
        public TimeSpan MTTR { get; }
        public TimeSpan TTRDispersion { get; }
        public DateTimeOffset Start { get; }
    }
    public class PartitionScheduleGenerator
    {
        private readonly IPartitionScheduleParameters _parameters;
        private readonly int _seed;
        public PartitionScheduleGenerator(IPartitionScheduleParameters parameters, int seed)
            => (_seed, _parameters) = (seed, parameters ?? throw new ArgumentNullException(nameof(parameters)));


        public IEnumerable<PartitionSchedule> Schedules
        {
            get
            {
                var r = new Random(_seed);
                while(true)
                    yield return new PartitionSchedule(_parameters, r.Next()); 
            }
        }
    }
}
