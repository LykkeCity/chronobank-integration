using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Repositories.Cashout;

namespace AzureRepositories.Cashout
{
    public class CashoutEntity : BaseEntity, ICashout
    {
        public Guid TransactionId => Guid.Parse(RowKey);
        public string Contract { get; set; }
        public decimal Amount { get; set; }

        public static string GeneratePartitionKey()
        {
            return "Cashout";
        }

        public static CashoutEntity Create(Guid id, string contract, decimal amount)
        {
            return new CashoutEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = id.ToString(),
                Contract = contract,
                Amount = amount
            };
        }
    }

    public class CashoutRepository : ICashoutRepository
    {
        private readonly INoSQLTableStorage<CashoutEntity> _storage;

        public CashoutRepository(INoSQLTableStorage<CashoutEntity> storage)
        {
            _storage = storage;
        }

        public Task CreateCashout(Guid id, string contract, decimal amount)
        {
            return _storage.InsertAsync(CashoutEntity.Create(id, contract, amount));
        }

        public async Task<ICashout> GetCashout(Guid id)
        {
            return await _storage.GetDataAsync(CashoutEntity.GeneratePartitionKey(), id.ToString());
        }
    }
}
