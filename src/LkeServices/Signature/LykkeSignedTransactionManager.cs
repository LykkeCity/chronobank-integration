using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Core.Providers;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.Web3;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.TransactionManagers;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.Signer;
using Transaction = Nethereum.Signer.Transaction;

namespace LkeServices.Signature
{
    public class LykkeSignedTransactionManager : ITransactionManager
    {
        private BigInteger DefaultGasPriceConst = BigInteger.Parse("20000000000");

        private BigInteger _nonceCount = -1;        
        private readonly ISignatureApi _signatureApi;

        public LykkeSignedTransactionManager(Web3 web3, ISignatureApi signatureApi)
        {
            _signatureApi = signatureApi;            
            Client = web3.Client;
        }

        public async Task<HexBigInteger> GetNonceAsync(TransactionInput transaction)
        {
            var ethGetTransactionCount = new EthGetTransactionCount(Client);
            var nonce = transaction.Nonce;
            if (nonce == null)
            {
                nonce = await ethGetTransactionCount.SendRequestAsync(transaction.From).ConfigureAwait(false);
                if (nonce.Value <= _nonceCount)
                {
                    _nonceCount = _nonceCount + 1;
                    nonce = new HexBigInteger(_nonceCount);
                }
                else
                    _nonceCount = nonce.Value;
            }
            return nonce;
        }

        public async Task<string> SendTransactionAsync<T>(T transaction) where T : TransactionInput
        {
            var ethSendTransaction = new EthSendRawTransaction(Client);

            var nonce = await GetNonceAsync(transaction);
            var value = transaction.Value?.Value ?? 0;
            var gasPrice = transaction.GasPrice?.Value ?? DefaultGasPriceConst;
            var gasValue = transaction.Gas?.Value ?? Transaction.DEFAULT_GAS_LIMIT;

            var tr = new Transaction(transaction.To, value, nonce, gasPrice, gasValue, transaction.Data);
            var hex = tr.GetRLPEncoded().ToHex();
            var response = await _signatureApi.SignTransaction(new SignRequest { From = transaction.From, Transaction = hex });

            return await ethSendTransaction.SendRequestAsync(response.SignedTransaction.EnsureHexPrefix()).ConfigureAwait(false);
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
