using System;
using System.Collections.Generic;

namespace Captain.Core
{
    public class PartitionSchedule
    {
        private IPartitionScheduleParameters _parameters;
        private Random _r;
        private int _seed;
        public PartitionSchedule(IPartitionScheduleParameters parameters, int seed)
            => (_seed, _parameters) = (seed, parameters ?? throw new ArgumentNullException(nameof(parameters)));
        public IEnumerable<(DateTimeOffset start, TimeSpan duration)> GetPartitions()
        {
            _r = new Random(_seed);
            var lastPartitionEnd = _parameters.Start;
            do
            {
                var start = lastPartitionEnd + GetNextRandomTTFSpan();

                var duration = GetNextRandomTTRSpan();
                lastPartitionEnd = start + duration;
                yield return (start, duration);
            } while (true);
        }
        private TimeSpan GetNextRandomTTFSpan()
        {
            var d = _r.NextDouble();
            return -Math.Log(1 - d) * _parameters.MTTF;
        }
        private TimeSpan GetNextRandomTTRSpan()
        {
            return _parameters.MTTR + _parameters.TTRDispersion * NextGaussian();
        }
        private double NextGaussian()
        {
            double v1, s;
            do
            {
                v1 = 2 * _r.NextDouble() - 1;
                var v2 = 2 * _r.NextDouble() - 1;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1 || s == 0);
            s = Math.Sqrt((-2 * Math.Log(s)) / s);

            return v1 * s;
        }
    }
}
