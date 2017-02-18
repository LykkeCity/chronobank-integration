using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Core.Contracts;
using Core.Ethereum;
using Core.Notifiers;
using Core.Settings;
using LkeServices.Triggers.Attributes;

namespace ChronobankJob.Functions
{
    public class GenerateUserContractPoolFunction
    {
        private readonly IUserContractQueueService _contractQueueService;
        private readonly IContractService _contractService;
        private readonly IPaymentService _paymentService;
        private readonly ILog _logger;
        private readonly BaseSettings _settings;
        private readonly IEmailNotifier _emailNotifier;
        private readonly ISlackNotifier _slackNotifier;

        private static DateTime _lastWarningSentTime = DateTime.MinValue;

        public GenerateUserContractPoolFunction(IUserContractQueueService contractQueueService, BaseSettings settings, IContractService contractService, IPaymentService paymentService, IEmailNotifier emailNotifier, ISlackNotifier slackNotifier, ILog logger)
        {
            _contractQueueService = contractQueueService;
            _settings = settings;
            _contractService = contractService;
            _paymentService = paymentService;
            _emailNotifier = emailNotifier;
            _slackNotifier = slackNotifier;
            _logger = logger;
        }

        [TimerTrigger("00:30:00")]
        public async Task Execute()
        {
            await InternalBalanceCheck();

            var currentCount = await _contractQueueService.Count();
            if (currentCount < _settings.MinContractPoolLength)
            {
                while (currentCount < _settings.MaxContractPoolLength)
                {
                    await InternalBalanceCheck();

                    var contracts = await _contractService.GenerateUserContracts(_settings.ContractsPerRequest);
                    foreach (var contract in contracts)
                        await _contractQueueService.PushContract(contract);

                    currentCount += _settings.ContractsPerRequest;
                }
            }
        }

        private async Task InternalBalanceCheck()
        {
            try
            {
                var balance = await _paymentService.GetMainAccountBalance();
                if (balance < _settings.MainAccountMinBalance)
                {
                    if ((DateTime.UtcNow - _lastWarningSentTime).TotalHours > 1)
                    {
                        string message = $"Main account {_settings.EthereumMainAccount} balance is less that {_settings.MainAccountMinBalance} ETH !";
                        await _logger.WriteWarningAsync("GenerateUserContractPoolFunction", "InternalBalanceCheck", "", message);

                        await _emailNotifier.WarningAsync("Chronobank integration", message);
                        await _slackNotifier.FinanceWarningAsync(message);

                        _lastWarningSentTime = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception e)
            {
                await _logger.WriteErrorAsync("GenerateUserContractPoolFunction", "InternalBalanceCheck", "", e);
            }
        }
    }
}
