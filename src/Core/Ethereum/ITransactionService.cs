using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace Core.Ethereum
{
    public interface ITransactionService
    {
        Task<bool> IsTransactionExecuted(string hash);
        Task<TransactionReceipt> GetTransactionReceipt(string transaction);
        Task<bool> WaitForExecution(string hash, int gasSended);
    }
}
