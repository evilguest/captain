using Captain.Core;
using System;


namespace Captain
{
    public abstract class RequestHandlerBase : IRequestHandler
    {
        protected readonly Random _random;
        protected bool _partitioned = false;
        protected readonly int _machineCount;
        protected decimal _balance;
        protected readonly decimal[] _nodeBalances;
        public RequestHandlerBase(int seed, int machineCount)
        {
            _random = new Random(seed);
            _machineCount = (machineCount > 0)
                ? machineCount
                : throw new ArgumentOutOfRangeException(nameof(machineCount), _machineCount, "Machine count must be above zero");
            //_balance = initialBalance;
            _nodeBalances = new decimal[machineCount];
        }

        public virtual TransferResult ProcessNormal(int machine, TransferRequest request) 
            => new TransferResult(request, _balance.CanAdd(request.Amount));

        public virtual TransferResult ProcessPartitioned(int machine, TransferRequest request) 
            => new TransferResult(request, _nodeBalances[machine].CanAdd(request.Amount));

        protected abstract decimal CollectBalances(decimal balance, decimal[] balances);

        protected abstract void DistributeBalances(decimal balance, decimal[] balances);


        #region IRequestHandler
        private TransferResult ProcessRequest(int machine, TransferRequest request) =>
            _partitioned
                ? ProcessPartitioned(machine, request)
                : ProcessNormal(machine, request);
        public TransferResult ProcessRequest(TransferRequest request)
            => ProcessRequest(_random.Next(_machineCount), request);


        public virtual void StartPartition()
        {
            DistributeBalances(_balance, _nodeBalances); 
            _partitioned = true;
        }

        public virtual void FinishPartition()
        {
            _balance = CollectBalances(_balance, _nodeBalances);
            _partitioned = false;
        }
        #endregion
    }
}