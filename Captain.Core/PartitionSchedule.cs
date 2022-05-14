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
                var lastPartitionEnd = _parameters.Start;
                do
                {
                    var ttf = GetNextRandomTTFSpan(r);
                    var ttr = GetNextRandomTTRSpan(r);

                    yield return new Partition(lastPartitionEnd + ttf, ttr);

                    lastPartitionEnd += ttf + ttr;

                } while (true);
            }
        }

        private TimeSpan GetNextRandomTTFSpan(Random r)
        {
            var d = r.NextDouble();
            return -Math.Log(1 - d) * _parameters.MTTF;
        }
        private TimeSpan GetNextRandomTTRSpan(Random r)
        {
            return _parameters.MTTR + _parameters.TTRDispersion * NextGaussian(r);
        }
        private static double NextGaussian(Random r)
        {
            double v1, s;
            do
            {
                v1 = 2 * r.NextDouble() - 1;
                var v2 = 2 * r.NextDouble() - 1;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1 || s == 0);
            s = Math.Sqrt((-2 * Math.Log(s)) / s);

            return v1 * s;
        }
    }
}
