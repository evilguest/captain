using Captain.Core;
using System.Collections;

namespace Captain
{
    internal class FilteredList<T>: IList<T>
    {
        private readonly IList<T> _list;
        private readonly IReadOnlyList<int> _index;
        public FilteredList(IList<T> list, IReadOnlyList<int> indexMap) =>
            (_list, _index) =
                (list ?? throw new ArgumentNullException(nameof(list)),
                indexMap ?? throw new ArgumentNullException(nameof(indexMap)));

        public T this[int index]
        {
            get => _list[_index[index]];
            set => _list[_index[index]] = value;
        }

        public int Count => _index.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public void Add(T item) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public bool Contains(T item) => throw new NotImplementedException();

        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<T> GetEnumerator()
        {
            for(var i=0; i<Count; i++)
                yield return _list[_index[i]];
        }

        public int IndexOf(T item) => throw new NotImplementedException();
        

        public void Insert(int index, T item) => throw new NotImplementedException();

        public bool Remove(T item) => throw new NotImplementedException();

        public void RemoveAt(int index) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
            
    public class CollectiveSplittingHandler : PartitionableHandler
    {
        public CollectiveSplittingHandler(int seed, int machineCount) : base(seed, machineCount)
        {
        }

        public override TransferResult ProcessPartitioned(int machine, TransferRequest request)
        {
            // we can get as much as possible from the nodes in the same part of the cluster
            var partBalance = 0m;
            for(var i = 0; i < _machineCount; i++)
            {
                if(NodePart(i) == NodePart(machine))
                    partBalance+=_nodeBalances[i];
            }
            if (partBalance.CanAdd(request.Amount))
            {
                BalancesInPart(NodePart(machine)).Distribute(partBalance);
                return request.Approve();
            }
            else return request.Reject();
        }
        private IList<decimal> BalancesInPart(ClusterPart part) =>
            new FilteredList<decimal>(_nodeBalances, GetPartMap(part));

        private IReadOnlyList<int> GetPartMap(ClusterPart part)
        {
            var t = new List<int>();
            for(var i= 0; i < _machineCount; i++)
                if(NodePart(i) == part)
                    t.Add(i);
            return t;
        }

        protected override decimal CollectBalances(decimal balance, decimal[] balances)
            => balances.Sum();


        protected override void DistributeBalances(decimal balance, decimal[] balances)
            => balances.Distribute(balance);
    }
}