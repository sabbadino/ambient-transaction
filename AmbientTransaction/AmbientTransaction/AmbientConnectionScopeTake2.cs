namespace AmbientTransaction
{
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;
    using Architect.AmbientContexts;
    using Microsoft.Data.SqlClient;

    public sealed class AmbientConnectionScopeTake2 : AsyncAmbientScope<AmbientConnectionScopeTake2>
    {
        private readonly string _connString;
        private readonly bool _ownsContext;
        private bool _vote;

        private DbConnection? _connection = null;
        private List<AmbientConnectionScopeTake2> ChildScopes = new List<AmbientConnectionScopeTake2>();

        public async Task<DbConnection> GetOpenConnectionOrCreate () { 
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connString);
                    await _connection.OpenAsync();
                    Transaction = await _connection.BeginTransactionAsync();
                }
                return _connection;
            } 
        

        public async Task<DbCommand> CreateCommandAsync (string text){ 
            var cmd = (await GetOpenConnectionOrCreate()).CreateCommand();
            cmd.Transaction = Transaction;
            cmd.CommandText = text;
            return cmd;
        }

        public DbTransaction? Transaction { get; private set; }

        private AmbientConnectionScopeTake2(AmbientScopeOption option, string connString, bool ownsContext)
            : base(option)
        {
            _ownsContext = ownsContext;
            _connString = connString;
            _vote = false;
        }

        public static AmbientConnectionScopeTake2 Create(string connString)
        {
            var existing = GetAmbientScope(false);
            if (existing!= null && existing?._connString != connString)
            {
                // TODO handle this better. Maybe keep info in a dictionary by connection string  
                throw new ArgumentException("The connection string does not match the one of the existing ambient scope.");    
            }
            var option = AmbientScopeOption.JoinExisting;

            var ownsContext = !(option == AmbientScopeOption.JoinExisting && existing != null);

            var scope = new AmbientConnectionScopeTake2(option, connString, ownsContext);
            // keep track of child scopes
            existing?.ChildScopes.Add(scope);    

            if (!ownsContext && existing != null)
            {
                scope._connection = existing._connection;
                scope.Transaction = existing.Transaction;
            }

            scope.Activate();

            return scope;
        }

        // TO BE TESTED
        public static AmbientConnectionScopeTake2 ForceCreateNew(string connString)
        {
            var option = AmbientScopeOption.ForceCreateNew;

            var ownsContext = true;
            var scope = new AmbientConnectionScopeTake2(option, connString, ownsContext);
            scope.Activate();
            return scope;
        }

        public static AmbientConnectionScopeTake2 Current =>
            GetAmbientScope(false) ?? throw new InvalidOperationException("No ambient connection scope active.");

        public void Complete()
        {
            _vote = true;
        }

        protected override void DisposeImplementation()
        {
            var x = DisposeAsyncImplementation();
            x.AsTask().Wait();
        }

        private void CheckChildScopes(AmbientConnectionScopeTake2 scope)
        {
            foreach (var childScope in scope.ChildScopes)
            {
                CheckChildScopes(childScope);
                if (!childScope._vote)
                {
                    if (scope._vote)
                    {
                        throw new Exception("A Child scope did not called complete, but parent scope voted to commit.");
                    }
                }
            }

        }

        protected override async ValueTask DisposeAsyncImplementation()
        {
            if (_ownsContext && _connection != null && Transaction != null)
            {
                try
                {
                    CheckChildScopes(this);
                }
                catch (Exception ex)
                {
                    await Transaction.RollbackAsync();
                    await _connection.DisposeAsync();
                    throw;
                }   
                if (_vote)
                    await Transaction.CommitAsync();
                else
                    await Transaction.RollbackAsync();

                await Transaction.DisposeAsync();
                await _connection.DisposeAsync();

                _connection = null;
                Transaction = null;

            }
        }
    }

}
