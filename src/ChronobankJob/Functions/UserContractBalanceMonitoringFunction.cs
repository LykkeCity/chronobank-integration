using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Common.Log;
using Core;
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
        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

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

                    if (balance > userContract.Balance || (DateTime.UtcNow - userContract.LastCheck) > Timeout)
                    {
                        await _logger.WriteInfoAsync("UserContractBalanceMonitoringFunction", "Process", $"Contract: {userContract.Address}, balance: {balance}, userBalace: {userContract.Balance}", "Start transfer");

                        var currentUserContract = _web3.Eth.GetContract(_settings.UserContract.Abi, userContract.Address);
                        var tx = await currentUserContract.GetFunction("transferMoney").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger(Constants.GasForTransfer),
                                        new HexBigInteger(0), _settings.EthereumMainAccount, balance - userContract.Balance);

                        await _userContractRepository.SetBalance(userContract.Address, balance);

                        await _transactionMonitoringQueueWriter.AddCashinToMonitoring(tx, userContract.Address, balance - userContract.Balance);

                        await _logger.WriteInfoAsync("UserContractBalanceMonitoringFunction", "Process", $"Contract: {userContract.Address}, balance: {balance}, userBalace: {userContract.Balance}", "End transfer");
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
