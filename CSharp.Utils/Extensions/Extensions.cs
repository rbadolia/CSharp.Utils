using System;
using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Extensions
{
    public static class Extensions
    {
        #region Public Methods and Operators

        public static HashSet<T2> BuildHashSet<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, T2> getValue)
        {
            var hashSet = new HashSet<T2>();
            foreach (T1 item in enumerable)
            {
                T2 value = getValue(item);
                if (value != null)
                {
                    hashSet.Add(value);
                }
            }

            return hashSet;
        }

        public static HashSet<T2> BuildHashSetOfValueType<T1, T2>(this IEnumerable<T1> enumerable, Func<T1, T2?> getValue) where T2 : struct
        {
            var hashSet = new HashSet<T2>();
            foreach (T1 item in enumerable)
            {
                T2? value = getValue(item);
                if (value != null)
                {
                    hashSet.Add(value.Value);
                }
            }

            return hashSet;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer = null)
        {
            Guard.ArgumentNotNull(enumerable, "enumerable");
            HashSet<T> hashSet;
            if (equalityComparer == null)
            {
                hashSet = new HashSet<T>(enumerable);
            }
            else
            {
                hashSet = new HashSet<T>(enumerable, equalityComparer);
            }

            return hashSet;
        }

        #endregion Public Methods and Operators
    }
}
