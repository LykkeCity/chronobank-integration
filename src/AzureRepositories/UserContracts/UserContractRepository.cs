using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Repositories.UserContracts;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.UserContracts
{
    public class UserContractEntity : TableEntity, IUserContract
    {
        public string Address => RowKey;

        public static string GeneratePartitionKey()
        {
            return "UserContract";
        }

        public static UserContractEntity Create(string address)
        {
            return new UserContractEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = address
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
    }
}
