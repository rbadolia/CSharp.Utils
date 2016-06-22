using CSharp.Utils.Collections.Concurrent;

namespace CSharp.Utils.Threading
{
    public abstract class AbstractFlyweightBasedDispatcherProvider : IDispatcherProvider
    {
        private readonly GenericFlyweight<IDispatcher> flyweight;

        protected AbstractFlyweightBasedDispatcherProvider(bool useWeakReferences)
        {
            this.flyweight = new GenericFlyweight<IDispatcher>(this.GetDispatcherCore, useWeakReferences);
        }

        protected abstract IDispatcher GetDispatcherCore(object target);

        public IDispatcher GetDispatcher(object target)
        {
            return this.flyweight.GetObject(target);
        }
    }
}
