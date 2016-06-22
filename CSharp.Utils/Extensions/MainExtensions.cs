using System.Collections.Generic;
using CSharp.Utils.Collections.Concurrent;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Extensions
{
    public static class MainExtensions
    {
        public static SafeListDecorator<T> ApplyThreadSafetyDecorator<T>(this IList<T> list)
        {
            return new SafeListDecorator<T>(list);
        }

        public static SafeCollectionDecorator<T> ApplyThreadSafetyDecorator<T>(this ICollection<T> collection)
        {
            return new SafeCollectionDecorator<T>(collection);
        }

        public static ListNotifyCollectionChangedAdapter<T> AdaptForNotifyCollectionChanged<T>(this IList<T> list)
        {
            return new ListNotifyCollectionChangedAdapter<T>(list);
        }

        public static DispatcherAwareListNotifyCollectionChangedAdapter<T> AdaptForDispatcherAwareNotifyCollectionChangedAdapter<T>(this IList<T> list, IDispatcherProvider dispatcherProvider)
        {
            return new DispatcherAwareListNotifyCollectionChangedAdapter<T>(list, dispatcherProvider);
        }

        public static IEnumerable<T> FilterDuplicates<T>(this IEnumerable<T> enumerable)
        {
            return new DuplicatesFilteredEnumerable<T>(enumerable);
        }

        public static IEnumerator<T> FilterDuplicates<T>(this IEnumerator<T> enumerator)
        {
            return new DuplicatesFilteredEnumerator<T>(enumerator);
        }
    }
}
