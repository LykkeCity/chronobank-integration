using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.TransactionMonitoring
{
    public class TransactionMonitoringMessage
    {
        public string TxHash { get; set; }
        public string UserContract { get; set; }
        public string Amount { get; set; }
    }
}
