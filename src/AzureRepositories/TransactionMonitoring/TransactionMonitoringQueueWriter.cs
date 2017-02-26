using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core;
using Core.TransactionMonitoring;

namespace AzureRepositories.TransactionMonitoring
{
    public class TransactionMonitoringQueueWriter : ITransactionMonitoringQueueWriter
    {
        private readonly IQueueExt _queue;

        public TransactionMonitoringQueueWriter(Func<string, IQueueExt> queueFactory)
        {
            _queue = queueFactory(Constants.TransactionMonitoringQueue);
        }

        public Task AddCashinToMonitoring(string txHash, string userContract, BigInteger amount)
        {
            return _queue.PutRawMessageAsync(new TransactionMonitoringMessage
            {
                UserContract = userContract,
                Amount = amount.ToString(),
                TxHash = txHash,
                Type = TransactionType.Cashin
            }.ToJson());
        }

        public Task AddCashoutToMonitoring(string txHash, string address)
        {
            return _queue.PutRawMessageAsync(new TransactionMonitoringMessage
            {
                Address = address,
                TxHash = txHash,
                Type = TransactionType.Cashout
            }.ToJson());
        }
    }
}
