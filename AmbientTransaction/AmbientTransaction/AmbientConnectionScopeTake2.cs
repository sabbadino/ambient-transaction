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

        public DbConnection Connection { get {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connString);
                    _connection.Open();
                    Transaction = _connection.BeginTransaction();
                }
                return _connection;
            } private set { _connection = value; }
        }

        public DbCommand CreateCommand (string text){ 
            var cmd = Connection.CreateCommand();
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
            var option = AmbientScopeOption.JoinExisting;

            var ownsContext = !(option == AmbientScopeOption.JoinExisting && existing != null);

            var scope = new AmbientConnectionScopeTake2(option, connString, ownsContext);

            existing?.ChildScopes.Add(scope);    

            if (!ownsContext && existing != null)
            {
                scope.Connection = existing.Connection;
                scope.Transaction = existing.Transaction;
            }

            scope.Activate();

            return scope;
        }

        //public static AmbientConnectionScopeTake2 SuppressScope(string connString)
        //{
        //    var scope = new AmbientConnectionScopeTake2(AmbientScopeOption.ForceCreateNew, connString, ownsContext: true);

        //    var conn = new SqlConnection(connString);
        //    await conn.OpenAsync().ConfigureAwait(false);
        //    scope.Connection = conn;
        //    scope.Transaction = conn.BeginTransaction();

        //    scope.Activate();


        //    return scope;
        //}

        public static AmbientConnectionScopeTake2 Current =>
            GetAmbientScope(false) ?? throw new InvalidOperationException("No ambient connection scope active.");

        public void Complete()
        {
            var x = this.GetHashCode();
            Console.WriteLine(x);
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
            if (_ownsContext && Connection != null && Transaction != null)
            {
                try
                {
                    CheckChildScopes(this);
                }
                catch (Exception ex)
                {
                    Transaction?.Rollback();
                    Connection.Dispose();
                    throw;
                }   
                if (_vote)
                    Transaction.Commit();
                else
                    Transaction.Rollback();

                Connection.Dispose();
            }
        }
    }

}
