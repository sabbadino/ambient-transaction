namespace AmbientTransaction
{
    using Architect.AmbientContexts;
    using System;
    using System.Threading.Tasks;

    public sealed class AmbientTransactionScope : AsyncAmbientScope<AmbientTransactionScope>
    {
        private readonly string _connString;
        private readonly bool _ownsContext;
        private bool _vote;

        private ConnectionInformation _ConnectionInformation;


        private List<AmbientTransactionScope> ChildScopes = new List<AmbientTransactionScope>();

        internal ConnectionInformation ConnectionInformation { get => _ConnectionInformation; }

        private void Setup()
        {
            _ConnectionInformation = new ConnectionInformation(_connString);    
        } 
       
        private AmbientTransactionScope(AmbientScopeOption option, string connString, bool ownsContext)
            : base(option)
        {
            ArgumentNullException.ThrowIfNull(connString);
            _ownsContext = ownsContext;
            _connString = connString;
            _vote = false;
        }

        public static AmbientTransactionScope Create(string connString)
        {
            ArgumentNullException.ThrowIfNull(connString);
            var existing = GetAmbientScope(false);
            if (existing!= null && existing._connString != connString)
            {
                // TODO handle this better. Maybe keep info in a dictionary by connection string  
                throw new ArgumentException("The connection string does not match the one of the existing ambient scope.");    
            }
            var option = AmbientScopeOption.JoinExisting;

            var scope = new AmbientTransactionScope(option, connString, existing==null);
            // keep track of child scopes
            if (existing != null)
            {
                existing.ChildScopes.Add(scope);
                scope._ConnectionInformation = existing._ConnectionInformation;
            }
            else {
                scope.Setup();
            }

            scope.Activate();

            return scope;
        }

        // TO BE TESTED
        public static AmbientTransactionScope ForceCreateNew(string connString)
        {
            var option = AmbientScopeOption.ForceCreateNew;

            var ownsContext = true;
            var scope = new AmbientTransactionScope(option, connString, ownsContext);
            scope.Activate();
            return scope;
        }

        public static AmbientTransactionScope? Current =>
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

        private void CheckChildScopes(AmbientTransactionScope scope)
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
            if(_ConnectionInformation==null)
            {
                throw new ObjectDisposedException("ConnectionInformation has been disposed");
            }
            if (_ConnectionInformation.IsInitialized && _ownsContext && _ConnectionInformation.Connection!= null && _ConnectionInformation.Transaction != null)
            {
                try
                {
                    CheckChildScopes(this);
                }
                catch (Exception ex)
                {
                    await _ConnectionInformation.Transaction.RollbackAsync();
                    await _ConnectionInformation.Connection.DisposeAsync();
                    throw;
                }   
                if (_vote)
                    await _ConnectionInformation.Transaction.CommitAsync();
                else
                    await _ConnectionInformation.Transaction.RollbackAsync();

                await _ConnectionInformation.Transaction.DisposeAsync();
                await _ConnectionInformation.Connection.DisposeAsync();

                _ConnectionInformation.Dispose();
            }
        }
    }

}
