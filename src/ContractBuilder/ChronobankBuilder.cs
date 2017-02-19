using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using LkeServices.Contracts;
using LkeServices.Ethereum;
using Nethereum.Contracts;
using Nethereum.Geth;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;

namespace ContractBuilder
{
    public enum ContractType
    {
        Platform,
        Asset,
        AssetProxy,
        Emmiter,
        EventHistory
    }

    public class ChronobankBuilder
    {
        private const string AssetName = "TIME";
        private const string Host = "http://localhost:8000";

        private readonly string _mainAccount;
        private readonly string _password;

        private readonly Web3 _web3;

        private Dictionary<ContractType, string> _addresses = new Dictionary<ContractType, string>();

        public ChronobankBuilder(string mainAccount, string password)
        {
            _mainAccount = mainAccount;
            _password = password;
            _web3 = new Web3(Host);

            ReadSettingsFile();
        }

        public async Task Build()
        {
            try
            {
                await _web3.Personal.UnlockAccount.SendRequestAsync(_mainAccount, _password, 600);

                await DeployContracts();

                WriteSettingsFile();

                await InitPlatformContract();
                await InitAssetContract();
                await InitAssetProxyContract();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }

        private async Task DeployContracts()
        {
            var platform = await DeployContract(ContractType.Platform);
            _addresses[ContractType.Platform] = platform;

            var asset = await DeployContract(ContractType.Asset);
            _addresses[ContractType.Asset] = asset;

            var assetProxy = await DeployContract(ContractType.AssetProxy);
            _addresses[ContractType.AssetProxy] = assetProxy;
        }

        private Task InitAssetContract()
        {
            var json = GetContract(ContractType.Asset);
            var contract = _web3.Eth.GetContract(json.Abi, _addresses[ContractType.Asset]);

            return ExecuteFunction(contract.GetFunction("init"), _addresses[ContractType.AssetProxy]);
        }

        private async Task InitAssetProxyContract()
        {
            var json = GetContract(ContractType.AssetProxy);
            var contract = _web3.Eth.GetContract(json.Abi, _addresses[ContractType.AssetProxy]);

            await ExecuteFunction(contract.GetFunction("init"), _addresses[ContractType.Platform], AssetName, AssetName);
            await ExecuteFunction(contract.GetFunction("proposeUpgrade"), _addresses[ContractType.Asset]);
        }

        private async Task InitPlatformContract()
        {
            var json = GetContract(ContractType.Platform);
            var contract = _web3.Eth.GetContract(json.Abi, _addresses[ContractType.Platform]);

            await ExecuteFunction(contract.GetFunction("issueAsset"), AssetName, 100000, AssetName, "description", 2, true);

            await ExecuteFunction(contract.GetFunction("setProxy"), _addresses[ContractType.AssetProxy], AssetName);
        }

        private async Task ExecuteFunction(Function function, params object[] inputs)
        {
            const int gas = 2000000;
            var tx = await function.SendTransactionAsync(_mainAccount, new HexBigInteger(gas), new HexBigInteger(0), inputs);

            TransactionReceipt receipt;
            while ((receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx)) == null)
            {
                await Task.Delay(100);
            }

            var transactionService = new TransactionService(new Web3Geth(Host), _web3);

            if (!await transactionService.IsTransactionExecuted(tx, gas))
                throw new Exception("Transaction was not executed");
        }

        private Task<string> DeployContract(ContractType type, params object[] p)
        {
            var contract = GetContract(type);
            var service = new ContractService(_web3, null);
            return service.CreateContract(_mainAccount, contract.Abi, contract.Bytecode, p);
        }

        private Contract GetContract(ContractType type)
        {
            string name;
            switch (type)
            {
                case ContractType.Asset:
                    name = "ChronoBankAsset";
                    break;
                case ContractType.AssetProxy:
                    name = "ChronoBankAssetProxy";
                    break;
                case ContractType.Platform:
                    name = "ChronobankPlatform";
                    break;
                case ContractType.Emmiter:
                    name = "ChronoBankPlatformEmitter";
                    break;
                case ContractType.EventHistory:
                    name = "EventsHistory";
                    break;
                default:
                    throw new Exception("bad type");

            }
            var file = File.ReadAllText($"./Contracts/bin/Chronobank/{name}.json");
            return JsonConvert.DeserializeObject<Contract>(file);
        }

        private void ReadSettingsFile()
        {
            var json = File.ReadAllText(@"..\..\settings\buildersettings.json");
            var data = JsonConvert.DeserializeObject<Settings>(json);
            _addresses = data.Addresses;
        }

        private void WriteSettingsFile()
        {
            var settings = new Settings
            {
                Addresses = _addresses
            };
            File.WriteAllText(@"..\..\settings\buildersettings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }

    public class Contract
    {
        public string Abi { get; set; }
        public string Bytecode { get; set; }
    }

    public class Settings
    {
        public Dictionary<ContractType, string> Addresses { get; set; }
    }
}
