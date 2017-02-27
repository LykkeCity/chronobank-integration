namespace Core.QueueReader
{
    public interface IConnectionPool
    {
        void AddConnection(string alias, string connectionString);
        string GetConnection(string alias);
    }
}
