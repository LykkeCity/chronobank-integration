using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.IssueNotifier
{
    public class IssueNotifyMessage
    {
        public string Contract { get; set; }

        public decimal Amount { get; set; }

        public string TransactionHash { get; set; }
    }
}
