using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Repositories.UserContracts
{
    public interface IUserContract
    {
        string Address { get; }
    }

    public interface IUserContractRepository
    {
        Task SaveContract(string address);
        Task<IEnumerable<IUserContract>> GetUsedContracts();
    }
}
