namespace AmbientTransaction
{
    using Architect.AmbientContexts;
    using Microsoft.Data.SqlClient;
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;
    public sealed class AmbientConnectionScopeTake2 : AsyncAmbientScope<AmbientConnectionScopeTake2>
    {
        private readonly string _connString;
        private readonly bool _ownsContext;
        private bool _vote;

        internal DbConnection? _actualConnection
            = null;

        internal DbTransaction? _actualTransaction;
        internal DbConnectionWrapper _connectionWrapper;
        private List<AmbientConnectionScopeTake2> ChildScopes = new List<AmbientConnectionScopeTake2>();

        private void Setup()
        {
                _actualConnection = new SqlConnection(_connString);
                _connectionWrapper = new DbConnectionWrapper(_actualConnection);
                _actualConnection.Open();
                _actualTransaction = _actualConnection.BeginTransaction();

        } 
        

        public DbCommand CreateCommand (string text){ 
            var cmd = _connectionWrapper.CreateCommand();
            cmd.Transaction = _actualTransaction;
            cmd.CommandText = text;
            return cmd;
        }

        

       // public DbTransaction? Transaction { get { return _TransactionWrapper; } }

     //   public DbConnection? Connection { get { return _connectionWrapper; } }

        private AmbientConnectionScopeTake2(AmbientScopeOption option, string connString, bool ownsContext)
            : base(option)
        {
            ArgumentNullException.ThrowIfNull(connString);
            _ownsContext = ownsContext;
            _connString = connString;
            _vote = false;
        }

        public static AmbientConnectionScopeTake2 Create(string connString)
        {
            ArgumentNullException.ThrowIfNull(connString);
            var existing = GetAmbientScope(false);
            if (existing!= null && existing._connString != connString)
            {
                // TODO handle this better. Maybe keep info in a dictionary by connection string  
                throw new ArgumentException("The connection string does not match the one of the existing ambient scope.");    
            }
            var option = AmbientScopeOption.JoinExisting;

            var scope = new AmbientConnectionScopeTake2(option, connString, existing==null);
            // keep track of child scopes
            if (existing != null)
            {
                existing.ChildScopes.Add(scope);
                scope._actualConnection = existing._actualConnection;
                scope._connectionWrapper= existing._connectionWrapper;
                scope._actualTransaction= existing._actualTransaction;
            }
            else {
                scope.Setup();
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

        public static AmbientConnectionScopeTake2? Current =>
            GetAmbientScope(false) ;

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
            if (_ownsContext && _actualConnection != null && _actualTransaction != null)
            {
                try
                {
                    CheckChildScopes(this);
                }
                catch (Exception ex)
                {
                    await _actualTransaction.RollbackAsync();
                    await _actualConnection.DisposeAsync();
                    throw;
                }   
                if (_vote)
                    await _actualTransaction.CommitAsync();
                else
                    await _actualTransaction.RollbackAsync();

                await _actualTransaction.DisposeAsync();
                await _actualConnection.DisposeAsync();

                _actualConnection = null;
                _actualTransaction = null;
            }
        }
    }

}
