using Captain.Core;
using System;


namespace Captain
{
    public abstract class TransactionHandlerBase : ITransactionHandler
    {
        protected readonly Random _random;
        protected bool _partitioned = false;
        protected readonly int _machineCount;
        protected decimal _balance;
        protected readonly decimal[] _nodeBalances;
        public TransactionHandlerBase(int seed, int machineCount, decimal initialBalance)
        {
            _random = new Random(seed);
            _machineCount = (machineCount > 0)
                ? machineCount
                : throw new ArgumentOutOfRangeException(nameof(machineCount), _machineCount, "Machine count must be above zero");
            _balance = initialBalance;
            _nodeBalances = new decimal[machineCount];
        }

        public virtual TransferResult ProcessNormal(int machine, TransferRequest request)
        {
            var confirmed = (_balance + request.Amount >= 0);
            if (confirmed)
                _balance += request.Amount;

            return new TransferResult(request.Id)
            {
                TimeStamp = request.TimeStamp,
                Amount = request.Amount,
                Balance = _balance,
                Confirmed = confirmed,
            };
        }

        public virtual TransferResult ProcessPartitioned(int machine, TransferRequest request)
        {
            var confirmed = (_nodeBalances[machine] + request.Amount >= 0);
            if (confirmed)
                _nodeBalances[machine] += request.Amount;

            return new TransferResult(request.Id)
            {
                TimeStamp = request.TimeStamp,
                Amount = request.Amount,
                Balance = _nodeBalances[machine],
                Confirmed = confirmed,
            };
        }

        public abstract decimal CollectBalances(decimal balance, decimal[] balances);

        public abstract void DistributeBalances(decimal balance, decimal[] balances);


        #region ITransactionHandler
        private TransferResult ProcessRequest(int machine, TransferRequest request) =>
            _partitioned
                ? ProcessPartitioned(machine, request)
                : ProcessNormal(machine, request);
        public TransferResult ProcessRequest(TransferRequest request) 
            => ProcessRequest(_random.Next(_machineCount), request);


        public virtual void StartPartition() => DistributeBalances(_balance, _nodeBalances);

        public virtual void FinishPartition() => _balance = CollectBalances(_balance, _nodeBalances);
        #endregion
    }
}