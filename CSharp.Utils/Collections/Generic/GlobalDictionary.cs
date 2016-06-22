using System;
using System.Collections.Generic;
using System.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    public static class GlobalDictionary
    {
        private static Dictionary<string, object> _dictionary = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        private static ReaderWriterLockSlim _slim=new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static void AddOrUpdate(string key, object value)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(key, "key");
            try
            {
                _slim.EnterWriteLock();
                _dictionary[key] = value;
            }
            finally
            {
                _slim.ExitWriteLock();
            }
        }

        public static object Get(string key)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(key, "key");
            try
            {
                _slim.EnterReadLock();
                object value;
                _dictionary.TryGetValue(key, out value);
                return value;
            }
            finally
            {
                _slim.ExitReadLock();
            }
        }
    }
}
