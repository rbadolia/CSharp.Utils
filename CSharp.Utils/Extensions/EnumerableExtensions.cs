using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharp.Utils.Collections.Concurrent;
using CSharp.Utils.Collections.Generic;

namespace CSharp.Utils.Extensions
{

    #region Delegates

    public delegate TKey GetKeyDelegate<out TKey, in TValue>(TValue value);

    #endregion Delegates

    public static class EnumerableExtensions
    {
        #region Static Fields

        private static readonly MethodInfo _castMethodInfo;

        #endregion Static Fields

        #region Constructors and Finalizers

        static EnumerableExtensions()
        {
            _castMethodInfo = typeof(Enumerable).GetMethod("Cast");
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

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

        public static IEnumerable Cast(this IEnumerable enumerable, Type targetType)
        {
            MethodInfo castedEnumerableGenericMethod = _castMethodInfo.MakeGenericMethod(targetType);
            return castedEnumerableGenericMethod.Invoke(null, new object[] { enumerable }) as IEnumerable;
        }

        public static IEnumerable<TKey> GetKeys<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return new KeyValuePairKeyEnumerable<TKey, TValue>(pairs);
        }

        public static IEnumerable<TValue> GetValeus<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return new KeyValuePairValueEnumerable<TKey, TValue>(pairs);
        }

        public static CounterEnumeratorDecorator<T> ToCounterEnumerator<T>(this IEnumerator<T> enumerator)
        {
            return new CounterEnumeratorDecorator<T>(enumerator);
        }

        public static IEnumerable<T> ToItemLockSafeEnumerable<T>(this IEnumerable<T> enumerable) where T : class
        {
            return new ItemLockSafeEnumerable<T>(enumerable);
        }

        #endregion Public Methods and Operators
    }
}
