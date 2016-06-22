namespace CSharp.Utils.Threading
{
    public sealed class DispatcherDependentBasedDispatcherProvider : IDispatcherProvider
    {
        private static readonly DispatcherDependentBasedDispatcherProvider InstanceObject = new DispatcherDependentBasedDispatcherProvider();

        private DispatcherDependentBasedDispatcherProvider()
        {
        }

        public static DispatcherDependentBasedDispatcherProvider Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public IDispatcher GetDispatcher(object target)
        {
            var dispatcherDependent = target as IDispatcherDependent;
            if (dispatcherDependent != null)
            {
                return dispatcherDependent.GetDispatcher();
            }

            return null;
        }
    }
}
