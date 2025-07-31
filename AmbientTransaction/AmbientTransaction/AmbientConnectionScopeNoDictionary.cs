//namespace AmbientTransaction
//{
//    using System;
//    using System.Data.Common;
//    using System.Threading.Tasks;
//    using Architect.AmbientContexts;
//    using Microsoft.Data.SqlClient;

//    public sealed class AmbientConnectionScopeNoDictionary : AsyncAmbientScope<AmbientConnectionScopeNoDictionary>
//    {
        
//        private readonly bool _ownsContext;
//        private bool _vote;
//        public AmbientConnectionScopeNoDictionary? Current2 { get { return CurrentAmbientScope?.Value; } }
//        public DbConnection? Connection { get; private set; }
//        public DbTransaction? Transaction { get; private set; }

//        private AmbientConnectionScopeNoDictionary(AmbientScopeOption option, string connString, bool ownsContext)
//            : base(option)
//        {
//            _ownsContext = ownsContext;
//            _vote = false;
//        }

//        public static async Task<AmbientConnectionScopeNoDictionary> CreateAsync(string connString)
//        {
//            var existing = GetAmbientScope(false);
//            var option = AmbientScopeOption.JoinExisting;

//            var ownsContext = !(option == AmbientScopeOption.JoinExisting && existing != null);

//            var scope = new AmbientConnectionScopeNoDictionary(option, connString, ownsContext);

//            if (ownsContext)
//            {
//                var conn = new SqlConnection(connString);
//                await conn.OpenAsync().ConfigureAwait(false);
//                scope.Connection = conn;
//                scope.Transaction = conn.BeginTransaction();
//            }
//            else if (existing != null)
//            {
//                scope.Connection = existing.Connection;
//                scope.Transaction = existing.Transaction;
//            }

//            scope.Activate();

//            return scope;
//        }

//        public static async Task<AmbientConnectionScopeNoDictionary> SuppressScopeAsync(string connString)
//        {
//            var scope = new AmbientConnectionScopeNoDictionary(AmbientScopeOption.ForceCreateNew, connString, ownsContext: true);

//            var conn = new SqlConnection(connString);
//            await conn.OpenAsync().ConfigureAwait(false);
//            scope.Connection = conn;
//            scope.Transaction = conn.BeginTransaction();

//            scope.Activate();


//            return scope;
//        }

//        public static AmbientConnectionScopeNoDictionary Current =>
//            GetAmbientScope(false) ?? throw new InvalidOperationException("No ambient connection scope active.");

//        public void Complete()
//        {
//            _vote = true;
//        }

//        protected override void DisposeImplementation()
//        {
//            if (_ownsContext && Connection != null && Transaction != null)
//            {
//                bool allVoted = true;
//                for (var s = this; s != null; s = s.PhysicalParentScope)
//                {
//                    if (!s._vote)
//                    {
//                        allVoted = false;
//                        break;
//                    }
//                }

//                if (allVoted)
//                    Transaction.Commit();
//                else
//                    Transaction.Rollback();

//                Connection.Dispose();
//            }
//        }

//        protected override async ValueTask DisposeAsyncImplementation()
//        {
//            if (_ownsContext && Connection is SqlConnection conn && Transaction != null)
//            {
//                bool allVoted = true;
//                for (var s = this; s != null; s = s.PhysicalParentScope)
//                {
//                    if (!s._vote)
//                    {
//                        allVoted = false;
//                        break;
//                    }
//                }

//                try
//                {
//                    if (allVoted)
//                        Transaction.Commit();
//                    else
//                        Transaction.Rollback();
//                }
//                finally
//                {
//                    await conn.DisposeAsync().ConfigureAwait(false);
//                }
//            }
//        }
//    }

//}
