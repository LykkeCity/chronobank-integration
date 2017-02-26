using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Core.TransactionMonitoring
{
    public interface ITransactionMonitoringQueueWriter
    {
        Task AddCashinToMonitoring(string txHash, string userContract, BigInteger amount);
        Task AddCashoutToMonitoring(string txHash, string address);
    }
}
