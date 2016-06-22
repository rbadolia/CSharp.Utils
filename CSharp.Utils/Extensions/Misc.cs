using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Contracts;

namespace CSharp.Utils.Extensions
{
    public static class Misc
    {
        #region Public Methods and Operators

        public static bool AddIfDoesNotExist<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }

            return false;
        }

        public static bool AddIfDoesNotExist<T, TValue>(this IDictionary<T, TValue> dictionary, T key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }

        public static Dictionary<string, T> BuildDictionary<T>(IEnumerable<T> uniqueItems) where T : IUnique
        {
            return uniqueItems.ToDictionary(item => item.Name);
        }

        public static TValue CreateIfDoesNotExistAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out bool isCreated) where TValue : new()
        {
            if (dictionary.ContainsKey(key))
            {
                isCreated = false;
                return dictionary[key];
            }

            var v = new TValue();
            dictionary.Add(key, v);
            isCreated = true;
            return v;
        }

        public static TValue CreateIfDoesNotExistAndGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            bool isCreated;
            return CreateIfDoesNotExistAndGet(dictionary, key, out isCreated);
        }

        public static Dictionary<string, T> ExportAsDictionary<T>(this IEnumerable<T> uniqueItems) where T : IUnique
        {
            return BuildDictionary(uniqueItems);
        }

        public static Dictionary<string, string> ExportAsDictionary(this NameValueCollection collection)
        {
            return GeneralHelper.BuildDictionaryFromNameValueCollection(collection);
        }

        public static Value GetIfExists<Key, Value>(this IDictionary<Key, Value> dictionary, Key key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return default(Value);
        }

        public static ChunkList<T> ToChunkList<T>(this IEnumerable<T> enumerable)
        {
            return new ChunkList<T>(enumerable);
        }

        #endregion Public Methods and Operators
    }
}
