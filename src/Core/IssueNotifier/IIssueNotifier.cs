using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.IssueNotifier
{
    public interface IIssueNotifier
    {
        Task AddNotify(string txHash, string contract, decimal amount);
    }
}
