using Captain.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Tests
{
    internal class TestHelper
    {
        internal static IEnumerable<TransferRequest> GetTopupThenWithdrawals(int count, decimal initialBalance, decimal withdrawal, TimeSpan interval)
        {
            var i = 0;
            var t = DateTime.Now;
            yield return new((i++).ToString(), t, initialBalance);
            t += interval;
            while (i < count)
            {
                yield return new(i.ToString(), t, -withdrawal);
                i++; t += interval;
            }

        }
    }
}
