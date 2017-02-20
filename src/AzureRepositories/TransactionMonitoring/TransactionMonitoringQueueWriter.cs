﻿using System;
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

        public Task AddToMonitoring(string txHash, string userContract, BigInteger amount)
        {
            return _queue.PutRawMessageAsync(new TransactionMonitoringMessage
            {
                UserContract = userContract,
                Amount = amount.ToString(),
                TxHash = txHash
            }.ToJson());
        }
    }
}