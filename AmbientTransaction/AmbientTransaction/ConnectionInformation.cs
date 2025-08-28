namespace AmbientTransaction
{
    using Microsoft.Data.SqlClient;
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    public sealed class ConnectionInformation : IDisposable, IAsyncDisposable
    {
        private readonly string _connString;
        private DbConnection? _actualConnection;
        private DbConnectionWrapper? _connectionWrapper;
        private DbTransaction? _transaction;
        internal string ConnectionString { get { return _connString; } }

        internal bool IsInitialized { get { return _actualConnection != null; } }

        internal DbConnection Connection { get {
                LazySetup();
                return _actualConnection!; } }

        public DbConnectionWrapper DbConnectionWrapper { get {
                LazySetup();
                return _connectionWrapper!; } }

        public DbTransaction Transaction { get {
                LazySetup();
                return _transaction!; } }

        public ConnectionInformation(string connString)
        {
            ArgumentNullException.ThrowIfNull(connString);
            _connString = connString;
        }

        private void LazySetup()
        {
            if (IsInitialized)
                return;
            try
            {
                _actualConnection = new SqlConnection(_connString);
                _connectionWrapper = new DbConnectionWrapper(_actualConnection);
                _actualConnection.Open();
                _transaction = _actualConnection.BeginTransaction();
            }
            catch (Exception ex)
            {
                // handle cases where connection could not be opened but BeginTransaction failed
                if (_actualConnection != null)
                {
                    try
                    {
                        _actualConnection.Dispose();
                    }
                    catch { }
                }
                _actualConnection = null;
                _connectionWrapper = null;
                _transaction = null;
                throw;
            }
        }

        public void Dispose()
        {
            var x = DisposeAsync();
            x.AsTask().Wait();

        }

        public ValueTask DisposeAsync()
        {
            _actualConnection = null;
            _connectionWrapper = null;  
            _transaction = null;    
            return ValueTask.CompletedTask; 
        }
    }

}
