using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Core.Settings;
using LkeServices.Ethereum;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Contracts;
using Nethereum.Geth;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace Tests
{
    [TestFixture]
    public class ContractTest
    {
        [Test]
        public async Task TestTransfer()
        {
            var settings = Config.Services.GetService<BaseSettings>();
            var web3 = Config.Services.GetService<Web3>();

            const string secondAccount = "0xd3c2dd7bee6345efd37873b1eb14e6ce6d976653";
            
            var contract = web3.Eth.GetContract(settings.ChronobankAssetProxy.Abi, settings.ChronobankAssetProxy.Address);

            var mainBalance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(settings.EthereumMainAccount);
            var userBalance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(secondAccount);

            const int amount = 4;
            await ExecuteFunction(settings, contract.GetFunction("transfer"), secondAccount, amount);

            var mainBalance2 = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(settings.EthereumMainAccount);
            var userBalance2 = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(secondAccount);

            Assert.AreEqual(mainBalance - amount, mainBalance2);
            Assert.AreEqual(userBalance + amount, userBalance2);
        }

        [Test]
        public async Task GetVersion()
        {
            var settings = Config.Services.GetService<BaseSettings>();

            var web3 = new Web3(settings.EthereumUrl);

            var contract = web3.Eth.GetContract(settings.ChronobankAssetProxy.Abi, settings.ChronobankAssetProxy.Address);

            var versionFor = await contract.GetFunction("getVersionFor").CallAsync<string>(settings.EthereumMainAccount);
            var versionLatest = await contract.GetFunction("getLatestVersion").CallAsync<string>();

            Assert.AreEqual(versionLatest, versionFor);
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
    }
}
