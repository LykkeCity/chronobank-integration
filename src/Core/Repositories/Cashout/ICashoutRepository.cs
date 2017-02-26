using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Repositories.Cashout
{
    public interface ICashout
    {
        Guid TransactionId { get; }
        string Contract { get; }
        decimal Amount { get; }
    }

    public interface ICashoutRepository
    {
        Task CreateCashout(Guid id, string contract, decimal amount);

        Task<ICashout> GetCashout(Guid id);
    }
}
