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
        public DateTimeOffset End { get; }
    }
    public class PartitionScheduleParameters : IPartitionScheduleParameters
    {
        public TimeSpan MTTF { get; set; }
        public TimeSpan MTTR { get; set; }
        public TimeSpan TTRDispersion { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }

    }
    public class PartitionScheduleGenerator
    {
        public PartitionScheduleGenerator(IPartitionScheduleParameters parameters) 
            => _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

        private readonly Random _r = new Random(42);
        private readonly IPartitionScheduleParameters _parameters;

        public IEnumerable<PartitionSchedule> Schedules
        {
            get
            {
                while(true)
                    yield return new PartitionSchedule(_parameters, _r.Next()); 
            }
        }
    }
}
