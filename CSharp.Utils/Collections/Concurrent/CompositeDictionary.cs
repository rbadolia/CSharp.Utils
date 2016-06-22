using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Concurrent
{

    #region Delegates

    public delegate TValue CreateObjectCallback<out TValue, in TKey>(TKey key);

    #endregion Delegates

    public sealed class CompositeDictionary<TKey, TValue> : AbstractDisposable, IAtomicOperationSupported
    {
        #region Fields

        private readonly TKey keyInParent;

        private readonly ReaderWriterLockSlim readerWriterLockSlim;

        private readonly CompositeDictionary<TKey, TValue> parent;

        private Dictionary<TKey, TValue> dictionary;

        private Dictionary<TKey, CompositeDictionary<TKey, TValue>> innerDictionaries;

        #endregion Fields

        #region Constructors and Finalizers

        public CompositeDictionary(bool shouldBeThreadSafe)
        {
            if (shouldBeThreadSafe)
            {
                this.readerWriterLockSlim = new ReaderWriterLockSlim();
            }
        }

        private CompositeDictionary(bool shouldThreadSafe, CompositeDictionary<TKey, TValue> parent, TKey keyInParent)
            : this(shouldThreadSafe)
        {
            this.parent = parent;
            this.keyInParent = keyInParent;
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public void AddOrUpdate(TValue value, params TKey[] keys)
        {
            this.AddOrUpdate(0, value, keys);
        }

        public void Clear()
        {
            if (this.readerWriterLockSlim != null)
            {
                this.readerWriterLockSlim.EnterWriteLock();
            }

            try
            {
                this.innerDictionaries = null;
                this.dictionary = null;
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public TValue Get(params TKey[] keys)
        {
            TValue value;
            this.TryGetValue(out value, keys);
            return value;
        }

        public List<CompositeKeyValue<TKey, TValue>> GetCompleteTree()
        {
            return this.GetCompleteTree(null, true);
        }

        public List<CompositeKeyValue<TKey, TValue>> GetCompleteTree(IComparer<CompositeKeyValue<TKey, TValue>> comparer, bool assending)
        {
            if (this.readerWriterLockSlim != null)
            {
                this.readerWriterLockSlim.EnterReadLock();
            }

            try
            {
                var compositeKeyValuesDictionary = new Dictionary<TKey, CompositeKeyValue<TKey, TValue>>();
                if (this.dictionary != null)
                {
                    foreach (var kvp in this.dictionary)
                    {
                        var composite = new CompositeKeyValue<TKey, TValue>(kvp);
                        compositeKeyValuesDictionary.Add(kvp.Key, composite);
                    }
                }

                if (this.innerDictionaries != null)
                {
                    foreach (var kvp in this.innerDictionaries)
                    {
                        CompositeKeyValue<TKey, TValue> composite;
                        if (!compositeKeyValuesDictionary.TryGetValue(kvp.Key, out composite))
                        {
                            composite = new CompositeKeyValue<TKey, TValue>(kvp.Key, default(TValue));
                            compositeKeyValuesDictionary.Add(kvp.Key, composite);
                        }

                        composite.InnerKeyValues = kvp.Value.GetCompleteTree(comparer, assending);
                        foreach (var compositeKeyValue in composite.InnerKeyValues)
                        {
                            compositeKeyValue.Parent = composite;
                        }
                    }
                }

                List<CompositeKeyValue<TKey, TValue>> result = compositeKeyValuesDictionary.Values.ToList();
                if (comparer != null)
                {
                    if (!assending)
                    {
                        comparer = new InverseComparer<CompositeKeyValue<TKey, TValue>>(comparer);
                    }

                    result.Sort(comparer);
                }

                return result;
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public List<TKey> GetKeys(params TKey[] keys)
        {
            return this.getKeys(0, keys);
        }

        public TValue GetOrAddAndGet(CreateObjectCallback<TValue, TKey> callback, params TKey[] keys)
        {
            return this.getOrAddAndGet(0, callback, keys);
        }

        public bool RemoveIfExists(params TKey[] keys)
        {
            return this.removeIfExists(0, keys);
        }

        public bool TryGetValue(out TValue value, params TKey[] keys)
        {
            return this.tryGetvalue(0, out value, keys);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.readerWriterLockSlim != null)
            {
                if (disposing)
                {
                    this.readerWriterLockSlim.EnterReadLock();
                }

                try
                {
                    if (this.innerDictionaries != null)
                    {
                        foreach (var kvp in this.innerDictionaries)
                        {
                            kvp.Value.Dispose();
                        }
                    }
                }
                finally
                {
                    if (disposing)
                    {
                        this.readerWriterLockSlim.ExitReadLock();
                    }
                }

                this.readerWriterLockSlim.Dispose();
            }
        }

        private void AddOrUpdate(int startIndex, TValue value, params TKey[] keys)
        {
            if (keys.Length - 1 == startIndex)
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.EnterWriteLock();
                }

                try
                {
                    if (this.dictionary == null)
                    {
                        this.dictionary = new Dictionary<TKey, TValue> { { keys[startIndex], value } };
                    }
                    else
                    {
                        this.dictionary[keys[startIndex]] = value;
                    }
                }
                finally
                {
                    if (this.readerWriterLockSlim != null)
                    {
                        this.readerWriterLockSlim.ExitWriteLock();
                    }
                }
            }
            else
            {
                CompositeDictionary<TKey, TValue> cd = this.getOrCreateGetCompositeDictionary(startIndex, keys);
                cd.AddOrUpdate(startIndex + 1, value, keys);
            }
        }

        private IEnumerable<TKey> getInnerKeys()
        {
            if (this.innerDictionaries != null)
            {
                return this.innerDictionaries.Keys;
            }

            return null;
        }

        private List<TKey> getKeys()
        {
            var list = new List<TKey>();
            if (this.dictionary != null)
            {
                list.AddRange(this.dictionary.Keys);
            }

            IEnumerable<TKey> innerList = this.getInnerKeys();
            if (innerList != null)
            {
                list.AddRange(innerList);
            }

            return list;
        }

        private List<TKey> getKeys(int startIndex, IList<TKey> keys)
        {
            if (keys == null || keys.Count == startIndex)
            {
                if (this.readerWriterLockSlim == null)
                {
                    return this.getKeys();
                }

                this.readerWriterLockSlim.EnterReadLock();
                try
                {
                    return this.getKeys();
                }
                finally
                {
                    this.readerWriterLockSlim.ExitReadLock();
                }
            }

            if (this.readerWriterLockSlim != null)
            {
                this.readerWriterLockSlim.EnterReadLock();
            }

            try
            {
                if (this.innerDictionaries == null)
                {
                    return new List<TKey>(0);
                }

                CompositeDictionary<TKey, TValue> cd;
                if (this.innerDictionaries.TryGetValue(keys[startIndex], out cd))
                {
                    return cd.getKeys(startIndex + 1, keys);
                }

                return new List<TKey>(0);
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        private TValue getOrAddAndGet(int startIndex, CreateObjectCallback<TValue, TKey> callback, params TKey[] keys)
        {
            if (keys.Length - 1 == startIndex)
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.EnterWriteLock();
                }

                try
                {
                    TValue value;
                    if (this.dictionary == null)
                    {
                        this.dictionary = new Dictionary<TKey, TValue>();
                        value = callback(keys[startIndex]);
                        this.dictionary.Add(keys[startIndex], value);
                        return value;
                    }

                    if (!this.dictionary.TryGetValue(keys[startIndex], out value))
                    {
                        value = callback(keys[startIndex]);
                        this.dictionary.Add(keys[startIndex], value);
                    }

                    return value;
                }
                finally
                {
                    if (this.readerWriterLockSlim != null)
                    {
                        this.readerWriterLockSlim.ExitWriteLock();
                    }
                }
            }

            CompositeDictionary<TKey, TValue> cd = this.getOrCreateGetCompositeDictionary(startIndex, keys);
            return cd.getOrAddAndGet(startIndex + 1, callback, keys);
        }

        private CompositeDictionary<TKey, TValue> getOrCreateGetCompositeDictionary(int startIndex, params TKey[] keys)
        {
            if (this.readerWriterLockSlim != null)
            {
                this.readerWriterLockSlim.EnterWriteLock();
            }

            try
            {
                CompositeDictionary<TKey, TValue> cd;
                if (this.innerDictionaries == null)
                {
                    this.innerDictionaries = new Dictionary<TKey, CompositeDictionary<TKey, TValue>>();
                    cd = new CompositeDictionary<TKey, TValue>(this.readerWriterLockSlim != null, this, keys[startIndex]);
                    this.innerDictionaries.Add(keys[startIndex], cd);
                }
                else
                {
                    if (!this.innerDictionaries.TryGetValue(keys[startIndex], out cd))
                    {
                        cd = new CompositeDictionary<TKey, TValue>(this.readerWriterLockSlim != null, this, keys[startIndex]);
                        this.innerDictionaries.Add(keys[startIndex], cd);
                    }
                }

                return cd;
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        private void OnInnerEmptied(CompositeDictionary<TKey, TValue> innerDictionary)
        {
            if (this.readerWriterLockSlim != null)
            {
                this.readerWriterLockSlim.EnterWriteLock();
            }

            try
            {
                this.innerDictionaries.Remove(innerDictionary.keyInParent);
                if (this.innerDictionaries.Count == 0)
                {
                    this.innerDictionaries = null;
                    if (this.parent != null && this.dictionary == null)
                    {
                        this.parent.OnInnerEmptied(this);
                    }
                }
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        private bool removeIfExists(int startIndex, params TKey[] keys)
        {
            if (keys.Length - 1 == startIndex)
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.EnterWriteLock();
                }

                try
                {
                    if (this.dictionary != null)
                    {
                        if (this.dictionary.Remove(keys[startIndex]))
                        {
                            if (this.dictionary.Count == 0)
                            {
                                this.dictionary = null;
                                if (this.parent != null && this.innerDictionaries == null)
                                {
                                    this.parent.OnInnerEmptied(this);
                                }
                            }

                            return true;
                        }
                    }

                    return false;
                }
                finally
                {
                    if (this.readerWriterLockSlim != null)
                    {
                        this.readerWriterLockSlim.ExitWriteLock();
                    }
                }
            }

            if (this.readerWriterLockSlim != null)
            {
                this.readerWriterLockSlim.EnterUpgradeableReadLock();
            }

            try
            {
                if (this.innerDictionaries != null)
                {
                    CompositeDictionary<TKey, TValue> cd;
                    if (this.innerDictionaries.TryGetValue(keys[startIndex], out cd))
                    {
                        return cd.removeIfExists(startIndex + 1, keys);
                    }
                }

                return false;
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitUpgradeableReadLock();
                }
            }
        }

        private bool tryGetvalue(int startIndex, out TValue value, params TKey[] keys)
        {
            if (keys.Length - 1 == startIndex)
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.EnterReadLock();
                }

                try
                {
                    if (this.dictionary != null)
                    {
                        return this.dictionary.TryGetValue(keys[startIndex], out value);
                    }

                    value = default(TValue);
                    return false;
                }
                finally
                {
                    if (this.readerWriterLockSlim != null)
                    {
                        this.readerWriterLockSlim.ExitReadLock();
                    }
                }
            }

            if (this.readerWriterLockSlim != null)
            {
                this.readerWriterLockSlim.EnterReadLock();
            }

            try
            {
                if (this.innerDictionaries == null)
                {
                    value = default(TValue);
                    return false;
                }
                else
                {
                    CompositeDictionary<TKey, TValue> cd;
                    if (this.innerDictionaries.TryGetValue(keys[startIndex], out cd))
                    {
                        return cd.tryGetvalue(startIndex + 1, out value, keys);
                    }

                    value = default(TValue);
                    return false;
                }
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        #endregion Methods

        public void PerformAtomicOperation(Action action)
        {
            Guard.ArgumentNotNull(action, "action");
            try
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.EnterWriteLock();
                }
            }
            finally
            {
                if (this.readerWriterLockSlim != null)
                {
                    this.readerWriterLockSlim.ExitWriteLock();
                }
            }
        }
    }
}
