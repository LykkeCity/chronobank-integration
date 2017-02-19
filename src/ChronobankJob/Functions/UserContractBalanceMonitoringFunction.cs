using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using Core.Repositories.UserContracts;
using Core.Settings;
using Core.TransactionMonitoring;
using LkeServices.Triggers.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;

namespace ChronobankJob.Functions
{
    public class UserContractBalanceMonitoringFunction
    {
        private readonly IUserContractRepository _userContractRepository;
        private readonly ILog _logger;
        private readonly Web3 _web3;
        private readonly BaseSettings _settings;
        private readonly ITransactionMonitoringQueueWriter _transactionMonitoringQueueWriter;

        public UserContractBalanceMonitoringFunction(IUserContractRepository userContractRepository, ILog logger, Web3 web3, BaseSettings settings, ITransactionMonitoringQueueWriter transactionMonitoringQueueWriter)
        {
            _userContractRepository = userContractRepository;
            _logger = logger;
            _web3 = web3;
            _settings = settings;
            _transactionMonitoringQueueWriter = transactionMonitoringQueueWriter;
        }

        [TimerTrigger("00:01:00")]
        public async Task Process()
        {
            var contracts = await _userContractRepository.GetUsedContracts();

            foreach (var userContract in contracts)
            {
                try
                {
                    var contract = _web3.Eth.GetContract(_settings.ChronobankAssetProxy.Abi, _settings.ChronobankAssetProxy.Address);

                    var balance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(userContract.Address);

                    if (balance > 0)
                    {
                        var currentUserContract = _web3.Eth.GetContract(_settings.UserContract.Abi, userContract.Address);
                        var tx = await currentUserContract.GetFunction("transferMoney").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger(200000),
                                        new HexBigInteger(0), _settings.EthereumMainAccount, balance);

                        await _transactionMonitoringQueueWriter.AddToMonitoring(tx, userContract.Address, balance);
                    }
                }
                catch (Exception e)
                {
                    await _logger.WriteErrorAsync("UserContractBalanceMonitoringFunction", "Process", userContract.Address, e);
                }
            }
        }
    }
}
