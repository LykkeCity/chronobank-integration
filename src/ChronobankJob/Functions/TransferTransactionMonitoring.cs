using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core;
using Core.Ethereum;
using Core.IssueNotifier;
using Core.Repositories.UserContracts;
using Core.Settings;
using Core.TransactionMonitoring;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.JobTriggers.Triggers.Bindings;

namespace ChronobankJob.Functions
{
    public class TransferTransactionMonitoring
    {
        private readonly IUserContractRepository _userContractRepository;
        private readonly ITransactionService _transactionService;
        private readonly IIssueNotifier _issueNotifier;
        private readonly ILog _logger;
        private readonly BaseSettings _settings;

        public TransferTransactionMonitoring(IUserContractRepository userContractRepository,
            ITransactionService transactionService, IIssueNotifier issueNotifier, ILog logger, BaseSettings settings)
        {
            _userContractRepository = userContractRepository;
            _transactionService = transactionService;
            _issueNotifier = issueNotifier;
            _logger = logger;
            _settings = settings;
        }

        [QueueTrigger(Constants.TransactionMonitoringQueue, notify: true)]
        public async Task Monitoring(TransactionMonitoringMessage message, QueueTriggeringContext context)
        {
            if (await _transactionService.IsTransactionExecuted(message.TxHash, Constants.GasForTransfer))
            {
                await _logger.WriteInfoAsync("TransferTransactionMonitoring", "Monitoring", message.ToJson(), "Transaction mined. Firing event.");

                if (message.Type == TransactionType.Cashin)
                {
                    var amount = BigInteger.Parse(message.Amount);

                    await _userContractRepository.DecreaseBalance(message.UserContract, amount);
                    await _issueNotifier.AddNotify(message.TxHash, message.UserContract, amount.FromBlockchainAmount(Constants.TimeCoinDecimals));

                    await _logger.WriteInfoAsync("TransferTransactionMonitoring", "Monitoring", message.ToJson(), "Cashin success");
                }

                if (message.Type == TransactionType.Cashout)
                {
                    await _logger.WriteInfoAsync("TransferTransactionMonitoring", "Monitoring", message.ToJson(), "Cashout success");
                }
            }
            else if ((DateTime.UtcNow - message.PutDateTime).TotalMinutes < _settings.TransactionExecutionTimeoutMinutes)
            {
                context.MoveMessageToEnd(message.ToJson());
                context.SetCountQueueBasedDelay(5000, 100);
            }
            else
            {
                context.MoveMessageToPoison();
                await _logger.WriteWarningAsync("TransferTransactionMonitoring", "Monitoring", message.ToJson(), "Transaction is failed");
            }
        }
    }
}
