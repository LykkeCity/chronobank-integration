using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class Constants
    {
        public const string EmailNotifierQueue = "emailsqueue";
        public const string SlackNotifierQueue = "slack-notifications";

        public const string UserContractQueue = "user-contracts";
        public const string TransactionMonitoringQueue = "transaction-monitoring";
    }
}
