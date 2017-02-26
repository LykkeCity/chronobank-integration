using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core;
using Core.IssueNotifier;

namespace AzureRepositories.IssueNotifier
{
    public class IssueNotifier : IIssueNotifier
    {
        private readonly IQueueExt _queue;

        public IssueNotifier(Func<string, IQueueExt> queueFactory)
        {
            _queue = queueFactory(Constants.IssueNotifyQueue);
        }

        public Task AddNotify(string txHash, string contract, decimal amount)
        {
            return _queue.PutRawMessageAsync(new IssueNotifyMessage
            {
                TransactionHash = txHash,
                Contract = contract,
                Amount = amount
            }.ToJson());
        }
    }
}
