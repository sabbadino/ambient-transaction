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


//    public sealed class XAmbientConnectionScopeNoAsync 
//    {
//        private static readonly ConcurrentDictionary<string, AsyncLocal<AmbientConnectionScopeNoAsync?>> AmbientByKey = new();
//        public static AmbientConnectionScopeNoAsync Create(string connString) {
//            var found = AmbientByKey.TryGetValue(connString, out var existing);
//            var newScope = AmbientConnectionScopeNoAsync.Create(connString, existing?.Value);
//            if (found)
//            {
//                AmbientByKey[connString] = new AsyncLocal<AmbientConnectionScopeNoAsync?> { Value = newScope };
//            }
//            else
//            {
//                AmbientByKey.TryAdd(connString, new AsyncLocal<AmbientConnectionScopeNoAsync?> { Value = newScope });
//            }
//            return newScope;
//        }
//        public static AmbientConnectionScopeNoAsync SuppressScope(string connString)
//        {
//            var found = AmbientByKey.TryGetValue(connString, out var existing);
//            var newScope = AmbientConnectionScopeNoAsync.SuppressScope(connString);
//            if (found)
//            {
//                AmbientByKey[connString] = new AsyncLocal<AmbientConnectionScopeNoAsync?> { Value = newScope };
//            }
//            else
//            {
//                AmbientByKey.TryAdd(connString, new AsyncLocal<AmbientConnectionScopeNoAsync?> { Value = newScope });
//            }
//            return newScope;
//        }

//    }
//    public sealed class AmbientConnectionScopeNoAsync : AsyncAmbientScope<AmbientConnectionScopeNoAsync>
//    {
//        private readonly bool _ownsContext;
//        private readonly string _connString;
//        private bool _vote;

//        public DbConnection? Connection { get; private set; }
//        public DbTransaction? Transaction { get; private set; }

//        private AmbientConnectionScopeNoAsync(AmbientScopeOption option, string connString, bool ownsContext)
//            : base(option)
//        {
//            _connString = connString;
//            _ownsContext = ownsContext;
//            _vote = false;
//        }

//        public static AmbientConnectionScopeNoAsync Create(string connString, AmbientConnectionScopeNoAsync? existing)
//        {
//            var option = AmbientScopeOption.JoinExisting;

//            var ownsContext = !(option == AmbientScopeOption.JoinExisting && existing != null);

//            var scope = new AmbientConnectionScopeNoAsync(option, connString, ownsContext);

//            if (ownsContext)
//            {
//                var conn = new SqlConnection(connString);
//                conn.Open();
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

//        public static AmbientConnectionScopeNoAsync SuppressScope(string connString)
//        {
//            var scope = new AmbientConnectionScopeNoAsync(AmbientScopeOption.ForceCreateNew, connString, ownsContext: true);

//            var conn = new SqlConnection(connString);
//            conn.Open();
//            scope.Connection = conn;
//            scope.Transaction = conn.BeginTransaction();

//            scope.Activate();
          

//            return scope;
//        }

//        public static AmbientConnectionScopeNoAsync Current =>
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
//                for (var s = this; s != null; s = s.PhysicalParentScope as AmbientConnectionScopeNoAsync)
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
//                for (var s = this; s != null; s = s.PhysicalParentScope as AmbientConnectionScopeNoAsync)
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
