using System;
using System.Collections.Generic;
using CSharp.Utils.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Concurrent
{
    public class GenericFlyweight<T> : AbstractMonitorAtomicOperationSupported where T : class
    {
        private readonly Dictionary<object, object> dictionary = new Dictionary<object, object>();

        private readonly Func<object, T> createObjectFunc;

        public GenericFlyweight(Func<object, T> createObjectFunc, bool useWeakReferences)
        {
            Guard.ArgumentNotNull(createObjectFunc, "createObjectFunc");
            this.createObjectFunc = createObjectFunc;
            this.UsesWeakReferences = useWeakReferences;
        }

        public bool UsesWeakReferences { get; private set; }

        public T GetObject(object flyweightSpecification)
        {
            Guard.ArgumentNotNull(flyweightSpecification, "flyweightSpecification");
            lock (this.SyncLockObject)
            {
                object obj;
                if (!this.dictionary.TryGetValue(flyweightSpecification, out obj))
                {
                    return this.CreateObjectAndAdd(flyweightSpecification);
                }

                T castedObject;
                if (this.TryCastObject(obj, out castedObject))
                {
                    return castedObject;
                }

                this.dictionary.Remove(flyweightSpecification);
                return this.CreateObjectAndAdd(flyweightSpecification);
            }
        }

        private bool TryCastObject(object obj, out T castedObject)
        {
            castedObject = null;
            if (obj == null)
            {
                return true;
            }

            if (!this.UsesWeakReferences)
            {
                castedObject = (T)obj;
                return true;
            }

            var weakReference = (WeakReference)obj;
            object target = weakReference.Target;
            if (target != null)
            {
                castedObject = (T)target;
                return true;
            }

            return false;
        }

        private T CreateObjectAndAdd(object flyweightSpecification)
        {
            T item = this.createObjectFunc(flyweightSpecification);
            object obj = item;
            if (item != null)
            {
                if (this.UsesWeakReferences)
                {
                    obj = new WeakReference(obj);
                }
            }

            this.dictionary.Add(flyweightSpecification, obj);
            return item;
        }

        public void Clear()
        {
            lock (this.SyncLockObject)
            {
                this.dictionary.Clear();
            }
        }

        public bool Remove(object flyweightSpecification)
        {
            lock (this.SyncLockObject)
            {
                return this.dictionary.Remove(flyweightSpecification);
            }
        }
    }
}
