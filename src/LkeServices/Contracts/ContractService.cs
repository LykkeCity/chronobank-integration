using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Contracts;
using Core.Settings;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace LkeServices.Contracts
{
    public class ContractService : IContractService
    {
        private readonly Web3 _web3;
        private readonly BaseSettings _settings;

        public ContractService(Web3 web3, BaseSettings settings)
        {
            _web3 = web3;
            _settings = settings;
        }

        public async Task<string> CreateContract(string from, string abi, string bytecode, params object[] constructorParams)
        {
            // deploy contract
            var transactionHash = await _web3.Eth.DeployContract.SendRequestAsync(abi, bytecode, from, new HexBigInteger(3000000), constructorParams);
            TransactionReceipt receipt;
            while ((receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash)) == null)
            {
                await Task.Delay(100);
            }

            // check if contract byte code is deployed
            var code = await _web3.Eth.GetCode.SendRequestAsync(receipt.ContractAddress);

            if (string.IsNullOrWhiteSpace(code) || code == "0x")
            {
                throw new Exception("Code was not deployed correctly, verify bytecode or enough gas was to deploy the contract");
            }

            return receipt.ContractAddress;
        }

        public async Task<string[]> GenerateUserContracts(int count = 10)
        {
            var transactionHashList = new List<string>();

            // sends <count> contracts
            for (var i = 0; i < count; i++)
            {
                // deploy contract
                var transactionHash = await
                        _web3.Eth.DeployContract.SendRequestAsync(_settings.UserContract.Abi, _settings.UserContract.ByteCode,
                            _settings.EthereumMainAccount, new HexBigInteger(500000), _settings.ChronobankAssetProxy.Address);

                transactionHashList.Add(transactionHash);
            }

            // wait for all <count> contracts transactions
            var contractList = new List<string>();
            for (var i = 0; i < count; i++)
            {
                // get contract transaction
                TransactionReceipt receipt;
                while ((receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHashList[i])) == null)
                {
                    await Task.Delay(100);
                }

                // check if contract byte code is deployed
                var code = await _web3.Eth.GetCode.SendRequestAsync(receipt.ContractAddress);

                if (string.IsNullOrWhiteSpace(code) || code == "0x")
                {
                    throw new Exception("Code was not deployed correctly, verify bytecode or enough gas was to deploy the contract");
                }

                contractList.Add(receipt.ContractAddress);
            }

            return contractList.ToArray();
        }
    }
}
