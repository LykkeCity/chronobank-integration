using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Core.Providers;
using Core.Settings;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.Web3;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.TransactionManagers;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.Signer;
using Nethereum.Util;
using Transaction = Nethereum.Signer.Transaction;

namespace LkeServices.Signature
{
    public class LykkeSignedTransactionManager : ITransactionManager
    {
        private readonly Web3 _web3;
        private readonly ISignatureApi _signatureApi;

        private readonly ConcurrentDictionary<string, INonceService> _nonceServices = new ConcurrentDictionary<string, INonceService>();
        private readonly BigInteger _minGasPrice;
        private readonly BigInteger _maxGasPrice;

        public LykkeSignedTransactionManager(Web3 web3, ISignatureApi signatureApi, BaseSettings baseSettings)
        {
            _web3 = web3;
            _signatureApi = signatureApi;
            Client = web3.Client;
            _minGasPrice = UnitConversion.Convert.ToWei(baseSettings.MinGasPriceGwei, UnitConversion.EthUnit.Gwei);
            _maxGasPrice = UnitConversion.Convert.ToWei(baseSettings.MaxGasPriceGwei, UnitConversion.EthUnit.Gwei);
        }

        private async Task<HexBigInteger> GetNonceAsync(TransactionInput transaction)
        {
            var nonce = transaction.Nonce;
            if (nonce == null)
            {
                var nonceService = _nonceServices.GetOrAdd(transaction.From, (key) => new InMemoryNonceService(key, Client));
                nonce = await nonceService.GetNextNonceAsync();
            }
            return nonce;
        }

        public async Task<string> SendTransactionAsync<T>(T transaction) where T : TransactionInput
        {
            var ethSendTransaction = new EthSendRawTransaction(Client);

            var nonce = await GetNonceAsync(transaction);
            var value = transaction.Value?.Value ?? 0;
            var gasPrice = await EstimateGasPrice();
            var gasValue = transaction.Gas?.Value ?? Transaction.DEFAULT_GAS_LIMIT;

            var tr = new Transaction(transaction.To, value, nonce, gasPrice, gasValue, transaction.Data);
            var hex = tr.GetRLPEncoded().ToHex();
            var response = await _signatureApi.SignTransaction(new SignRequest { From = transaction.From, Transaction = hex });

            return await ethSendTransaction.SendRequestAsync(response.SignedTransaction.EnsureHexPrefix()).ConfigureAwait(false);
        }

        public async Task<BigInteger> EstimateGasPrice()
        {
            var currentGasPrice = (await _web3.Eth.GasPrice.SendRequestAsync()).Value;
            return BigInteger.Max(BigInteger.Min(currentGasPrice, _maxGasPrice), _minGasPrice);
        }


        public Task<HexBigInteger> EstimateGasAsync<T>(T callInput) where T : CallInput
        {
            throw new NotImplementedException();
        }

        public Task<string> SendTransactionAsync(string @from, string to, HexBigInteger amount)
        {
            throw new NotImplementedException();
        }

        public IClient Client { get; set; }

        public BigInteger DefaultGasPrice { get; set; }
        public BigInteger DefaultGas { get; set; }

        public ITransactionReceiptService TransactionReceiptService { get; set; }
    }
}
