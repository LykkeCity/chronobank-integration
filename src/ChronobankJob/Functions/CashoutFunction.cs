using System;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core;
using Core.Repositories.Cashout;
using Core.Settings;
using Core.TransactionMonitoring;
using LkeServices.Triggers.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;

namespace ChronobankJob.Functions
{
    public class CashoutFunction
    {
        private readonly ICashoutRepository _cashoutRepository;
        private readonly ILog _logger;
        private readonly Web3 _web3;
        private readonly BaseSettings _settings;
        private readonly ITransactionMonitoringQueueWriter _transactionMonitoringQueueWriter;

        public CashoutFunction(ICashoutRepository cashoutRepository, ILog logger, Web3 web3, BaseSettings settings, ITransactionMonitoringQueueWriter transactionMonitoringQueueWriter)
        {
            _cashoutRepository = cashoutRepository;
            _logger = logger;
            _web3 = web3;
            _settings = settings;
            _transactionMonitoringQueueWriter = transactionMonitoringQueueWriter;
        }

        [QueueTrigger("chronobank-out", notify: true)]
        public async Task Process(CashoutModel model)
        {
            if (await _cashoutRepository.GetCashout(model.Id) != null)
            {
                await _logger.WriteWarningAsync("CashoutFunction", "Process", "Cashout already exists", model.ToJson());
                return;
            }

            await _cashoutRepository.CreateCashout(model.Id, model.Address, model.Amount);

            await _logger.WriteInfoAsync("CashoutFunction", "Process", $"Begin cashout [{model.Id}]", model.ToJson());

            var contract = _web3.Eth.GetContract(_settings.ChronobankAssetProxy.Abi, _settings.ChronobankAssetProxy.Address);

            var amount = model.Amount.ToBlockchainAmount(Constants.TimeCoinDecimals);

            var tx = await contract.GetFunction("transfer").SendTransactionAsync(_settings.EthereumMainAccount, new HexBigInteger(Constants.GasForTransfer),
                                        new HexBigInteger(0), _settings.EthereumMainAccount, model.Address, amount);

            await _transactionMonitoringQueueWriter.AddCashoutToMonitoring(tx, model.Address);

            await _logger.WriteInfoAsync("CashoutFunction", "Process", $"End cashout [{model.Id}]", "");
        }
    }

    public class CashoutModel
    {
        public Guid Id { get; set; }
        public string Address { get; set; }
        public decimal Amount { get; set; }
    }
}
