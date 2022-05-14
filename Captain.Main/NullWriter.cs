using Captain.Core;

namespace Captain.Main
{
    internal class NullWriter : IStatisticsWriter
    {
        public void Dispose()
        {
            
        }

        public void WriteResult(StatisticsItem item)
        {
            // do nothing;
        }
    }
}