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
        public int Seed { get; }
    }
    public class PartitionScheduleGenerator
    {
        public PartitionScheduleGenerator(IPartitionScheduleParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _r = new Random(_parameters.Seed);
        }

        private readonly Random _r;
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
