using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Ethereum;
using Core.Settings;
using Nethereum.Geth;
using Nethereum.Geth.RPC.Debug.DTOs;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace LkeServices.Ethereum
{
    public class TransactionService : ITransactionService
    {
        private readonly Web3Geth _web3Geth;
        private readonly Web3 _web3;

        public TransactionService(Web3Geth web3Geth, Web3 web3)
        {
            _web3Geth = web3Geth;
            _web3 = web3;
        }

        public async Task<bool> IsTransactionExecuted(string hash)
        {
            var receipt = await GetTransactionReceipt(hash);
            return receipt != null && receipt.Status.Value.IsOne;
        }

        public async Task<TransactionReceipt> GetTransactionReceipt(string transaction)
        {
            return await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction);
        }

        public async Task<bool> WaitForExecution(string hash, int gasSended)
        {
            while (await GetTransactionReceipt(hash) == null)
                await Task.Delay(100);
            return await IsTransactionExecuted(hash);
        }
    }

    public class TansactionTrace
    {
        public int Gas { get; set; }
        public string ReturnValue { get; set; }
        public TransactionStructLog[] StructLogs { get; set; }
    }

    public class TransactionStructLog
    {
        public object Error { get; set; }
    }
}
