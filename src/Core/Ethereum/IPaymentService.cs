using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Ethereum
{
    public interface IPaymentService
    {
        Task<decimal> GetMainAccountBalance();
    }
}
