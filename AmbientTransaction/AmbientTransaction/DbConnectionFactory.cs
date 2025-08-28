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

            if (AmbientTransactionScope.Current != null ) {
                if(AmbientTransactionScope.Current.ConnectionInformation.ConnectionString != ConnectionString) {
                    throw new ArgumentException("The connection string of DbConnectionFactory does not match the one of the existing ambient scope.");
                }
                // unfortunately we cannot avoid to pass the actual dbtransaction here , since it has to be associated with the command
                dbTransaction = AmbientTransactionScope.Current?.ConnectionInformation.Transaction;
                ArgumentNullException.ThrowIfNull(dbTransaction, "AmbientTransactionScope exists but no transaction found. This should not happen.");   
                return AmbientTransactionScope.Current!.ConnectionInformation.DbConnectionWrapper;
            }
            else
            {
                var cn = new SqlConnection(ConnectionString) ;
                dbTransaction = null;
                return cn;
            }
            } 
        public string ConnectionString { get; }

        
    }
}
