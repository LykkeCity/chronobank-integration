using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Core;
using Core.Contracts;
using Core.Exceptions;
using Core.Notifiers;
using Core.Repositories.UserContracts;

namespace LkeServices.Contracts
{
    public class UserContractQueueService : IUserContractQueueService
    {
        private readonly ISlackNotifier _slackNotifier;
        private readonly IQueueExt _queue;
        private readonly IUserContractRepository _userContractRepository;

        public UserContractQueueService(ISlackNotifier slackNotifier, Func<string, IQueueExt> queueFactory, IUserContractRepository userContractRepository)
        {
            _slackNotifier = slackNotifier;
            _userContractRepository = userContractRepository;
            _queue = queueFactory(Constants.UserContractQueue);
        }

        public async Task<string> GetContract()
        {
            var contract = await GetContractRaw();

            await _userContractRepository.SaveContract(contract);

            return contract;
        }

        public async Task<string> GetContractRaw()
        {
            Action throwAction = () =>
            {
                _slackNotifier.ErrorAsync("Chronobank integration! User contract pool is empty!");
                throw new BackendException("User contract pool is empty!", ErrorCode.ContractPoolEmpty);
            };
            var message = await _queue.GetRawMessageAsync();
            if (message == null)
                throwAction();

            await _queue.FinishRawMessageAsync(message);

            var contract = message.AsString;

            if (string.IsNullOrWhiteSpace(contract))
                throwAction();

            return contract;
        }

        public Task PushContract(string contract)
        {
            return _queue.PutRawMessageAsync(contract);
        }

        public async Task<int> Count()
        {
            return await _queue.Count() ?? 0;
        }
    }
}
