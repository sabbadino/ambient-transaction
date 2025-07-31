//namespace AmbientTransaction
//{
//    using System;
//    using System.Collections.Concurrent;
//    using System.Data;
//    using System.Data.Common;
//    using System.Data.SqlClient;
//    using System.Threading;
//    using System.Threading.Tasks;
//    using Architect.AmbientContexts;
//    using Microsoft.Data.SqlClient;


//    public sealed class XAmbientConnectionScope 
//    {
//        private static readonly ConcurrentDictionary<string, AsyncLocal<AmbientConnectionScope?>> AmbientByKey = new();
//        public static async Task<AmbientConnectionScope> CreateAsync(string connString) {
//            var found = AmbientByKey.TryGetValue(connString, out var existing);
//            var newScope = await AmbientConnectionScope.CreateAsync(connString, existing?.Value);
//            if (found)
//            {
//                AmbientByKey[connString] = new AsyncLocal<AmbientConnectionScope?> { Value = newScope };
//            }
//            else
//            {
//                AmbientByKey.TryAdd(connString, new AsyncLocal<AmbientConnectionScope?> { Value = newScope });
//            }
//            return newScope;
//        }
//        public static async Task<AmbientConnectionScope> SuppressScopeAsync(string connString)
//        {
//            var found = AmbientByKey.TryGetValue(connString, out var existing);
//            var newScope = await AmbientConnectionScope.SuppressScopeAsync(connString);
//            if (found)
//            {
//                AmbientByKey[connString] = new AsyncLocal<AmbientConnectionScope?> { Value = newScope };
//            }
//            else
//            {
//                AmbientByKey.TryAdd(connString, new AsyncLocal<AmbientConnectionScope?> { Value = newScope });
//            }
//            return newScope;
//        }

//    }
//    public sealed class AmbientConnectionScope : AsyncAmbientScope<AmbientConnectionScope>
//    {
//        private readonly bool _ownsContext;
//        private bool _vote;
//        public AmbientConnectionScope? Current2 { get { return CurrentAmbientScope?.Value; } }
//        public DbConnection? Connection { get; private set; }
//        public DbTransaction? Transaction { get; private set; }

//        private AmbientConnectionScope(AmbientScopeOption option, string connString, bool ownsContext)
//            : base(option)
//        {
//            _ownsContext = ownsContext;
//            _vote = false;
//        }

//        public static async Task<AmbientConnectionScope> CreateAsync(string connString, AmbientConnectionScope? existing)
//        {
//            var option = AmbientScopeOption.JoinExisting;

//            var ownsContext = !(option == AmbientScopeOption.JoinExisting && existing != null);

//            var scope = new AmbientConnectionScope(option, connString, ownsContext);

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

//        public static async Task<AmbientConnectionScope> SuppressScopeAsync(string connString)
//        {
//            var scope = new AmbientConnectionScope(AmbientScopeOption.ForceCreateNew, connString, ownsContext: true);

//            var conn = new SqlConnection(connString);
//            await conn.OpenAsync().ConfigureAwait(false);
//            scope.Connection = conn;
//            scope.Transaction = conn.BeginTransaction();

//            scope.Activate();
          

//            return scope;
//        }

//        public static AmbientConnectionScope Current =>
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
//                for (var s = this; s != null; s = s.PhysicalParentScope as AmbientConnectionScope)
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
//                for (var s = this; s != null; s = s.PhysicalParentScope as AmbientConnectionScope)
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
