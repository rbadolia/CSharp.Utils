using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using CSharp.Utils.Contracts;
using CSharp.Utils.Extensions;
using CSharp.Utils.Reflection;

namespace CSharp.Utils
{
    public static class GeneralHelper
    {
        #region Static Fields

        private static long _identity;

        #endregion Static Fields

        #region Public Properties

        [StateIntact]
        public static long Identity
        {
            get
            {
                return Interlocked.Increment(ref _identity);
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        [CautionUsedByReflection]
        public static void SafeInvoke(Delegate del, params object[] arguments)
        {
            if (del == null)
            {
                return;
            }

            var invocationList = del.GetInvocationList();
            if (invocationList.Length == 1)
            {
                try
                {
                    del.DynamicInvoke(arguments);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            else
            {
                foreach (var innerDelegate in invocationList)
                {
                    SafeInvoke(innerDelegate, arguments);
                }
            }
        }

        public static void AbortThread(Thread t)
        {
            try
            {
                t.Abort();
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static Dictionary<TKey, TValue> BuildDictionary<TKey, TValue>(IEnumerable<TValue> enumerable, GetKeyDelegate<TKey, TValue> getKeyDelegate)
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (TValue value in enumerable)
            {
                TKey key = getKeyDelegate(value);
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        public static Dictionary<string, string> BuildDictionaryFromNameValueCollection(NameValueCollection collection)
        {
            var dictionary = new Dictionary<string, string>();
            for (int i = 0; i < collection.Count; i++)
            {
                string key = collection.GetKey(i);
                string value = collection[i];
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        public static void DisposeIDisposable(object obj)
        {
            if (obj != null)
            {
                var disposable = obj as IDisposable;
                if (disposable != null)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        public static void DisposeIDisposables(IEnumerable disposables)
        {
            foreach (IDisposable disposable in disposables)
            {
                DisposeIDisposable(disposable);
            }
        }

        public static Dictionary<string, string> GetApplicationSettings()
        {
            var settings = new Dictionary<string, string>();
            for (int i = 0; i < ConfigurationManager.AppSettings.Count; i++)
            {
                settings.Add(ConfigurationManager.AppSettings.GetKey(i), ConfigurationManager.AppSettings[i]);
            }

            return settings;
        }

        public static void InitializeInitializable(object obj)
        {
            if (obj != null)
            {
                var initializable = obj as IInitializable;
                if (initializable != null)
                {
                    try
                    {
                        initializable.Initialize();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        throw;
                    }
                }
            }
        }

        public static void InitializeInitializables(IEnumerable enumerable)
        {
            foreach (object obj in enumerable)
            {
                InitializeInitializable(obj);
            }
        }

        #endregion Public Methods and Operators

        [CautionUsedByReflection]
        public static int Compare(object x, object y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x != null && y != null)
            {
                var comparable = x as IComparable;
                if (comparable != null)
                {
                    return comparable.CompareTo(y);
                }

                if (x.Equals(y))
                {
                    return 0;
                }

                return string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
            }

            return x == null ? -1 : 1;
        }
    }
}
