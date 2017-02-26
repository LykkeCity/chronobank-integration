using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.TransactionMonitoring
{
    public enum TransactionType
    {
        Cashin,
        Cashout
    }

    public class TransactionMonitoringMessage
    {
        public string TxHash { get; set; }
        public string UserContract { get; set; }
        public string Address { get; set; }
        public string Amount { get; set; }
        public TransactionType Type { get; set; }
    }
}
