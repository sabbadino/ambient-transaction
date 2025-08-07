using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace AmbientTransaction
{
    public interface IDbConnectionFactory
    {
        DbConnection GetOpenConnection(out DbTransaction? dbTransaction);
        string ConnectionString { get; }

    }
    public class DbConnectionFactory : IDbConnectionFactory
    {
        public DbConnectionFactory(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ConnectionString = connectionString;
        }

        public DbConnection GetOpenConnection (out DbTransaction? dbTransaction) {

            // unfortunately we cannot avoid to pass the actual dbtransaction here , since it has to be associated with the command
            dbTransaction = AmbientTransactionScope.Current?._actualTransaction;
            if(AmbientTransactionScope.Current!=null)
            {
                return AmbientTransactionScope.Current._connectionWrapper;
            }
            else
            {
                var cn = new SqlConnection(ConnectionString) ;
                cn.OpenAsync();
                return cn;
            }
            } 
        string ConnectionString { get; }

        string IDbConnectionFactory.ConnectionString => ConnectionString;
    }
}
