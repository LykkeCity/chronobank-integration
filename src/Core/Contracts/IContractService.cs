using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IContractService
    {
        Task<string> CreateContract(string from, string abi, string bytecode, params object[] constructorParams);
        Task<string[]> GenerateUserContracts(int count = 10);
    }
}
