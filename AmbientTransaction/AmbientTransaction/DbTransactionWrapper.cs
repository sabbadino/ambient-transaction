namespace AmbientTransaction
{
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    public class DbTransactionWrapper : DbTransaction
    {
        internal readonly DbTransaction _innerTransaction;
        public DbTransactionWrapper(DbTransaction innerTransaction)
        {
            _innerTransaction = innerTransaction;
        }
        public override IsolationLevel IsolationLevel => _innerTransaction.IsolationLevel;
        public override void Commit()
        {
            _innerTransaction.Commit();
        }
        public override void Rollback()
        {
            _innerTransaction.Rollback();
        }
        

        public override string ToString()
        {
            return _innerTransaction.ToString();
        }

        public override ValueTask DisposeAsync()
        {
            return _innerTransaction.DisposeAsync();
        }
        public override Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _innerTransaction.CommitAsync(cancellationToken);
        }

        public override bool Equals(object? obj)
        {
            return _innerTransaction.Equals(obj);
        }
        public override int GetHashCode()
        {
            return _innerTransaction.GetHashCode();
        }
        public override void Release(string savepointName)
        {
            _innerTransaction.Release(savepointName);
        }
        public override Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            return _innerTransaction.ReleaseAsync(savepointName, cancellationToken);
        }
        public override void Rollback(string savepointName)
        {
            _innerTransaction.Rollback(savepointName);
        }
        public override Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return _innerTransaction.RollbackAsync(cancellationToken);
        }
        public override Task RollbackAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            return _innerTransaction.RollbackAsync(savepointName, cancellationToken);
        }
        public override void Save(string savepointName)
        {
            _innerTransaction.Save(savepointName);
        }



        public override Task SaveAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            return _innerTransaction.SaveAsync(savepointName, cancellationToken);
        }

        public override bool SupportsSavepoints => _innerTransaction.SupportsSavepoints;

        protected override DbConnection? DbConnection => _innerTransaction.Connection;
        protected override void Dispose(bool disposing)
        {
            _innerTransaction.Dispose();
        }
    }

}
