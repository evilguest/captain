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
            _parameters = parameters
                          ?? throw new ArgumentNullException(nameof(parameters));
        }

        private readonly IPartitionScheduleParameters _parameters;

        public IEnumerable<PartitionSchedule> Schedules
        {
            get
            {
                var r = new Random(_parameters.Seed);
                while(true)
                    yield return new PartitionSchedule(_parameters, r.Next()); 
            }
        }
    }
}
