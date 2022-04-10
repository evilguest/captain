using System.Collections.Generic;

namespace Captain.Core
{
    public interface IRequestReader
    {
        public IEnumerable<TransferRequest> Read(string fileName, string language);
    }
}
