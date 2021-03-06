﻿using System;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Core;
using Core.Notifiers;
using Newtonsoft.Json;
using Lykke.JobTriggers.Abstractions;

namespace AzureRepositories.Notifiers
{
	public class SlackNotifier : ISlackNotifier, IPoisionQueueNotifier
    {
		private readonly IQueueExt _queue;

		public SlackNotifier(Func<string, IQueueExt> queueFactory)
		{
			_queue = queueFactory(Constants.SlackNotifierQueue);
		}

		public async Task WarningAsync(string message)
		{
			var obj = new
			{
                Type = "Warnings",
                Sender = "chronobank service",
                Message = message
			};
            
			await _queue.PutRawMessageAsync(JsonConvert.SerializeObject(obj));
		}

        public async Task ErrorAsync(string message)
        {
            var obj = new
            {
                Type = "Errors",
                Sender = "chronobank service",
                Message = message
            };

            await _queue.PutRawMessageAsync(JsonConvert.SerializeObject(obj));
        }

        public async Task FinanceWarningAsync(string message)
        {
            var obj = new
            {
                Type = "Financewarnings",
                Sender = "chronobank service",
                Message = message
            };

            await _queue.PutRawMessageAsync(JsonConvert.SerializeObject(obj));
        }

        public Task NotifyAsync(string message)
        {
            return ErrorAsync(message);
        }
    }
}
