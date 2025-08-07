using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace AmbientTransaction
{
    public interface IDbConnectionFactory
    {
        DbConnection GetConnection(out DbTransaction? dbTransaction);
        string ConnectionString { get; }

    }
    public class DbConnectionFactory : IDbConnectionFactory
    {
        public DbConnectionFactory(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ConnectionString = connectionString;
        }

        public DbConnection GetConnection (out DbTransaction? dbTransaction) {

            dbTransaction = AmbientConnectionScopeTake2.Current?._actualTransaction;
            return AmbientConnectionScopeTake2.Current?._connectionWrapper ?? (new SqlConnection(ConnectionString)) as DbConnection;   
            } 
        string ConnectionString { get; }

        string IDbConnectionFactory.ConnectionString => ConnectionString;
    }
}
