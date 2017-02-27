using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Core.QueueReader;

namespace AzureRepositories.QueueReader
{
    public class ConnectionPool : IConnectionPool
    {
        private readonly Dictionary<string,string> _connections = new Dictionary<string, string>();

        public void AddConnection(string alias, string connectionString)
        {
            _connections[alias] = connectionString;
        }

        public string GetConnection(string alias)
        {
            if (!_connections.ContainsKey(alias))
                throw new Exception($"Connection alias '{alias}' is not registered");
            return _connections[alias];
        }
    }
}
