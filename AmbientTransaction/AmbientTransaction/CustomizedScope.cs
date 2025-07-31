namespace AmbientTransaction
{
    using Architect.AmbientContexts;

    public sealed class CustomizedScope : AmbientScope<CustomizedScope>
    {
        public CustomizedScope()
            : base(AmbientScopeOption.NoNesting)
        {
        }

        protected override void DisposeImplementation()
        {
        }

        public new void Activate()
        {
            base.Activate();
        }
    }



}
