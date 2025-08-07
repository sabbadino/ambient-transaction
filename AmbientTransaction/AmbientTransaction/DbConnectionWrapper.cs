namespace AmbientTransaction
{
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

        public class DbConnectionWrapper : DbConnection
    {
        internal readonly DbConnection _innerConnection;
        public DbConnectionWrapper(DbConnection innerConnection)
        {
            _innerConnection = innerConnection;
            _innerConnection.StateChange += InnerConnection_StateChange;
        }

        private void InnerConnection_StateChange(object? sender, StateChangeEventArgs e)
        {
            _stateChangeHandlers?.Invoke(this, e);
        }

        public override string ConnectionString
        {
            get => _innerConnection.ConnectionString; set => throw new NotSupportedException();
        }

        public override bool CanCreateBatch => _innerConnection.CanCreateBatch;

        public override Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            return _innerConnection.ChangeDatabaseAsync(databaseName, cancellationToken);
        }
        public override Task CloseAsync()
        {
            return Task.CompletedTask;  
        }

        public override int ConnectionTimeout => _innerConnection.ConnectionTimeout;

        public override ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public override void EnlistTransaction(System.Transactions.Transaction? transaction)
        {
            throw new InvalidOperationException("Do not try to EnlistTransaction a transaction explicitly. Use AmbientConnectionScope");
        }
        public override bool Equals(object? obj)
        {
            return _innerConnection.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _innerConnection.GetHashCode();
        }

        public override DataTable GetSchema()
        {
            return _innerConnection.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return _innerConnection.GetSchema(collectionName);
        }
        public override DataTable GetSchema(string collectionName, string?[] restrictionValues)
        {
            return _innerConnection.GetSchema(collectionName, restrictionValues);
        }

        

        public override Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return _innerConnection.GetSchemaAsync(cancellationToken);
        }

        public override Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            return _innerConnection.GetSchemaAsync(collectionName, cancellationToken);
        }

        public override Task<DataTable> GetSchemaAsync(string collectionName, string?[] restrictionValues, CancellationToken cancellationToken = default)
        {
            return _innerConnection.GetSchemaAsync(collectionName, restrictionValues, cancellationToken);
        }

      

        public override ISite? Site { get => _innerConnection.Site; set => _innerConnection.Site = value; }
        public override event StateChangeEventHandler? StateChange
        {
            add
            {
                _stateChangeHandlers += value;
            }
            remove
            {
                _stateChangeHandlers -= value;
            }
        }

        private StateChangeEventHandler? _stateChangeHandlers;

        public override string ToString()
        {
            return _innerConnection.ToString();
        }

public override string Database => _innerConnection.Database;
        public override string DataSource => _innerConnection.DataSource;
        public override string ServerVersion => _innerConnection.ServerVersion;
        public override ConnectionState State => _innerConnection.State;
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new InvalidOperationException("Do not try to start a transaction explicitly. Use AmbientConnectionScope"); 
        }
        protected override DbCommand CreateDbCommand() => _innerConnection.CreateCommand();
        public override void Open() { return; }
        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        protected override void Dispose(bool disposing)
        {
            return;
        }

      

        public override void ChangeDatabase(string databaseName)
        {
            _innerConnection.ChangeDatabase(databaseName);  
        }

        public override void Close()
        {
            return;
        }
    }

}
