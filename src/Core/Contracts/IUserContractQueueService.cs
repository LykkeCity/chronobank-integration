using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IUserContractQueueService
    {
        Task<string> GetContract();
        Task PushContract(string contract);
        Task<int> Count();
    }
}
