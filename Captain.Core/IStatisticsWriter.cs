using System;

namespace Captain.Core
{
    public record class StatisticsItem(string Scheduler, double C, double A, double P)
    {
        public override string ToString() => $"Scheduler: {Scheduler} C: {C:p2} A: {A:p2} P: {P:p2}";
    }
    public interface IStatisticsWriter: IDisposable
    {
        void WriteResult(StatisticsItem item);
    }
}