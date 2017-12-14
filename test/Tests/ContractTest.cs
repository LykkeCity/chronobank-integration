using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AzureRepositories.Cashout;
using AzureStorage.Tables;
using Core.Ethereum;
using Core.Settings;
using Core.TransactionMonitoring;
using LkeServices.Ethereum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Queue;
using Nethereum.Contracts;
using Nethereum.Geth;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;
using Xunit;

namespace Tests
{
    public class ContractTest
    {
        [Fact]
        public async Task TestTransfer()
        {
            var settings = Config.Services.GetService<BaseSettings>();
            var web3 = Config.Services.GetService<Web3>();

            var transactionService = Config.Services.GetService<ITransactionService>();
            var receipt =
                await transactionService.GetTransactionReceipt(
                    "0xf73c722e5d874389957b1600d2da6887806daf21a2c7c3873dd9cd6359eed4f9");

            var receipt2 =
                await transactionService.GetTransactionReceipt(
                    "0x48e2cca5c4ec7f79f9ace5246efe7bc95f5453cde9e841314d66a7eb157bb463");

            const string secondAccount = "0xd3c2dd7bee6345efd37873b1eb14e6ce6d976653";
            
            var contract = web3.Eth.GetContract(settings.ChronobankAssetProxy.Abi, settings.ChronobankAssetProxy.Address);

            var mainBalance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(settings.EthereumMainAccount);
            var userBalance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(secondAccount);

            const int amount = 4;
            await ExecuteFunction(settings, contract.GetFunction("transfer"), secondAccount, amount);

            var mainBalance2 = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(settings.EthereumMainAccount);
            var userBalance2 = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(secondAccount);

            Assert.Equal(mainBalance - amount, mainBalance2);
            Assert.Equal(userBalance + amount, userBalance2);
        }

        [Fact]
        public async Task GetVersion()
        {
            var settings = Config.Services.GetService<BaseSettings>();

            var web3 = new Web3(settings.EthereumUrl);

            var contract = web3.Eth.GetContract(settings.ChronobankAssetProxy.Abi, settings.ChronobankAssetProxy.Address);

            var versionFor = await contract.GetFunction("getVersionFor").CallAsync<string>(settings.EthereumMainAccount);
            var versionLatest = await contract.GetFunction("getLatestVersion").CallAsync<string>();

            Assert.Equal(versionLatest, versionFor);
        }

        private async Task ExecuteFunction(BaseSettings settings, Function function, params object[] inputs)
        {
            var web3 = new Web3(settings.EthereumUrl);

            const int gas = 2000000;
            var tx = await function.SendTransactionAsync(settings.EthereumMainAccount, new HexBigInteger(gas), new HexBigInteger(0), inputs);

            TransactionReceipt receipt;
            while ((receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx)) == null)
            {
                await Task.Delay(100);
            }

            var transactionService = new TransactionService(new Web3Geth(settings.EthereumUrl), web3);

            if (!await transactionService.IsTransactionExecuted(tx, gas))
                throw new Exception("Transaction was not executed");
        }

        public async Task Do()
        {
            var cashoutRepo = new CashoutRepository(new AzureTableStorage<CashoutEntity>("", "Cashouts", null));
        }
    }
}
