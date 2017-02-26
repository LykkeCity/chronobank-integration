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
using Core.TransactionMonitoring;
using LkeServices.Triggers.Attributes;

namespace ChronobankJob.Functions
{
    public class TransferTransactionMonitoring
    {
        private readonly IUserContractRepository _userContractRepository;
        private readonly ITransactionService _transactionService;
        private readonly IIssueNotifier _issueNotifier;
        private readonly ILog _logger;

        public TransferTransactionMonitoring(IUserContractRepository userContractRepository,
            ITransactionService transactionService, IIssueNotifier issueNotifier, ILog logger)
        {
            _userContractRepository = userContractRepository;
            _transactionService = transactionService;
            _issueNotifier = issueNotifier;
            _logger = logger;
        }

        [QueueTrigger(Constants.TransactionMonitoringQueue)]
        public async Task Monitoring(TransactionMonitoringMessage message)
        {
            if (await _transactionService.WaitForExecution(message.TxHash, Constants.GasForTransfer))
            {
                await _logger.WriteInfoAsync("TransferTransactionMonitoring", "Monitoring", message.ToJson(), "Transaction mined. Firing event.");

                if (message.Type == TransactionType.Cashin)
                {
                    var amount = BigInteger.Parse(message.Amount);

                    await _userContractRepository.DecreaseBalance(message.UserContract, amount);
                    await _issueNotifier.AddNotify(message.TxHash, message.UserContract, amount.FromBlockchainAmount(Constants.TimeCoinDecimals));

                    await _logger.WriteInfoAsync("TransferTransactionMonitoring", "Monitoring", "Cashin success", message.ToJson());
                }

                if (message.Type == TransactionType.Cashout)
                {
                    await _logger.WriteInfoAsync("TransferTransactionMonitoring", "Monitoring", "Cashout success", message.ToJson());
                }
            }
            else
            {
                await _logger.WriteWarningAsync("TransferTransactionMonitoring", "Monitoring", message.ToJson(), "Transaction is failed");
            }
        }
    }
}
