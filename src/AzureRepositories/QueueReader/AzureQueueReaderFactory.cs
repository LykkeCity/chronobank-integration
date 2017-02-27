using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Core.QueueReader;
using IQueueReader = Core.QueueReader.IQueueReader;

namespace AzureRepositories.QueueReader
{
    public class AzureQueueReaderFactory : IQueueReaderFactory
    {
        private readonly IConnectionPool _connectionPool;        

	    public AzureQueueReaderFactory(IConnectionPool connectionPool)
	    {
	        _connectionPool = connectionPool;	        
	    }

	    public IQueueReader Create(string connection, string queueName)
	    {		   
		    return new AzureQueueReader(new AzureQueueExt(_connectionPool.GetConnection(connection), queueName));
	    }        
    }
}
