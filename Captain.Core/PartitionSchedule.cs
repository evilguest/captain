using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;

namespace Captain.Core
{
    public record struct Partition(DateTimeOffset start, TimeSpan duration)
    {
        public DateTimeOffset finish { get => start + duration; }
    }
    public class PartitionSchedule
    {
        private IPartitionScheduleParameters _parameters;
        private int _seed;
        public PartitionSchedule(IPartitionScheduleParameters parameters, int seed)
            => (_seed, _parameters) = (seed, parameters ?? throw new ArgumentNullException(nameof(parameters)));
        public IEnumerable<Partition> Partitions
        {
            get
            {
                var r = new Random(_seed);
                var logNormal = LogNormal.WithMeanVariance(_parameters.MTTR.TotalSeconds, _parameters.TTRDispersion.TotalSeconds, r);
                var lastPartitionEnd = _parameters.Start;
                do
                {
                    var ttf = GetNextRandomTTFSpan(r);
                    var ttr = TimeSpan.FromSeconds(logNormal.Sample());

                    yield return new Partition(lastPartitionEnd + ttf, ttr);

                    lastPartitionEnd += ttf + ttr;

                } while (true);
            }
        }

        private TimeSpan GetNextRandomTTFSpan(Random r) 
            => TimeSpan.FromSeconds(Exponential.Sample(r, 1.0 / _parameters.MTTF.TotalSeconds));
    }
}
