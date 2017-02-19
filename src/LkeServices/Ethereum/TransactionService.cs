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

        public async Task<bool> IsTransactionExecuted(string hash, int gasSended)
        {
            var receipt = await GetTransactionReceipt(hash);

            if (receipt == null)
                return false;

            if (receipt.GasUsed.Value != new Nethereum.Hex.HexTypes.HexBigInteger(gasSended).Value)
                return true;
            
            var logs = await _web3Geth.Debug.TraceTransaction.SendRequestAsync(hash, new TraceTransactionOptions());

            var obj = logs.ToObject<TansactionTrace>();
            if (obj.StructLogs?.Length > 0 && obj.StructLogs[obj.StructLogs.Length - 1].Error != null)
                return false;

            return true;
        }
        
        public async Task<TransactionReceipt> GetTransactionReceipt(string transaction)
        {
            return await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction);
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
