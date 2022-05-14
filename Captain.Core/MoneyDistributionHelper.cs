using System.Collections.Generic;
using System.Linq;

namespace Captain
{
    public static class MoneyDistributionHelper
    {
        public static void Distribute(this IList<decimal> balances, decimal balance)        
        {
            int cents = (int)(balance * 100);
            var share = cents / balances.Count;
            var rem = cents % balances.Count;
            for (int i = 0; i < rem; i++)
                balances[i] = ((decimal)(share + 1)) / 100;
            for (int i = rem; i < balances.Count; i++)
                balances[i] = ((decimal)share) / 100;
            // now we need to fix up the possible remainder of the cents rounding
            balances[0] += balance - ((decimal)cents) / 100;
        }
        public static void Equalize(this IList<decimal> balances, decimal balance)
        {
            for(var i = 0; i< balances.Count; i++)
                balances[i] = balance;
        }

        public static decimal CollectDeltas(this IEnumerable<decimal> balances, decimal initialBalance) =>
            balances.Sum() - initialBalance * (balances.Count() - 1);
        public static decimal Collect(this IEnumerable<decimal> balances) => balances.Sum();
    }
}