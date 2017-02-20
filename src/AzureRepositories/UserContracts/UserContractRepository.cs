using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AzureStorage;
using Core.Repositories.UserContracts;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.UserContracts
{
    public class UserContractEntity : TableEntity, IUserContract
    {
        public string Address => RowKey;

        public string BalanceStr { get; set; }

        public BigInteger Balance => string.IsNullOrEmpty(BalanceStr)? BigInteger.Zero : BigInteger.Parse(BalanceStr);

        public DateTime LastCheck { get; set; }

        public static string GeneratePartitionKey()
        {
            return "UserContract";
        }

        public static UserContractEntity Create(string address)
        {
            return new UserContractEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = address,
                LastCheck = DateTime.UtcNow
            };
        }
    }

    public class UserContractRepository : IUserContractRepository
    {
        private readonly INoSQLTableStorage<UserContractEntity> _storage;

        public UserContractRepository(INoSQLTableStorage<UserContractEntity> storage)
        {
            _storage = storage;
        }

        public Task SaveContract(string address)
        {
            return _storage.InsertAsync(UserContractEntity.Create(address));
        }

        public async Task<IEnumerable<IUserContract>> GetUsedContracts()
        {
            return await _storage.GetDataAsync(UserContractEntity.GeneratePartitionKey());
        }

        public Task SetBalance(string address, BigInteger balance)
        {
            return _storage.ReplaceAsync(UserContractEntity.GeneratePartitionKey(), address, entity =>
            {
                entity.BalanceStr = balance.ToString();
                entity.LastCheck = DateTime.UtcNow;
                return entity;
            });
        }

        public Task DecreaseBalance(string address, BigInteger amount)
        {
            return _storage.ReplaceAsync(UserContractEntity.GeneratePartitionKey(), address, entity =>
            {
                entity.BalanceStr = BigInteger.Max(0, entity.Balance - amount).ToString();                
                return entity;
            });
        }
    }
}
