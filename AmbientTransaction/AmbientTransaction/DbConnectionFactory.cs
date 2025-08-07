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

            // unfortunately we cannot avoid to pass the actual dbtransaction here , since it has to be associated with the command
            dbTransaction = AmbientConnectionScopeTake2.Current?._actualTransaction;
            if(AmbientConnectionScopeTake2.Current!=null)
            {
                return AmbientConnectionScopeTake2.Current._connectionWrapper;
            }
            else
            {
                var cn = new SqlConnection(ConnectionString) ;
                cn.Open();
                return cn;
            }
            } 
        string ConnectionString { get; }

        string IDbConnectionFactory.ConnectionString => ConnectionString;
    }
}
