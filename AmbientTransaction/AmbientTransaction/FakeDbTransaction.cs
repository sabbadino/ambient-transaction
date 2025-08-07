using System.Data;
using System.Data.Common;

namespace AmbientTransaction
{
    internal class FakeDbTransaction : DbTransaction
    {
        private DbConnectionWrapper dbConnectionWrapper;

        public FakeDbTransaction(DbConnectionWrapper dbConnectionWrapper)
        {
            this.dbConnectionWrapper = dbConnectionWrapper;
        }

        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

        protected override DbConnection? DbConnection => dbConnectionWrapper;

        public override void Commit()
        {
            return;
        }

        public override void Rollback()
        {
            return; // No operation, as this is a fake transaction
        }

        
    }
}