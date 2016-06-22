using System;
using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Algorithms
{
    public static class InPlaceMergeSortHelper
    {
        public static void MergeSort<T>(this IList<T> list)
        {
            MergeSort(list, Comparer<T>.Default);
        }

        public static void MergeSort<T>(this IList<T> list, IComparer<T> comparer)
        {
            Guard.ArgumentNotNull(comparer, "comparer");
            MergeSort(list, comparer, 0, list.Count - 1);
        }

        public static void MergeSort<T>(this IList<T> list, Comparison<T> comparison)
        {
            Guard.ArgumentNotNull(comparison, "comparison");
            var comparer = new DelegateBasedComparer<T>(comparison);
            MergeSort(list, comparer);
        }

        private static void MergeSort<T>(IList<T> list, IComparer<T> comparer, int fromPos, int toPos)
        {
            Guard.ArgumentNotNull(list, "list");
            Guard.ArgumentNotNull(comparer, "comparer");
            MergeSortCore(list, comparer, fromPos, toPos);
        }

        private static void MergeSortCore<T>(IList<T> list, IComparer<T> comparer, int fromPos, int toPos)
        {
            if (fromPos < toPos)
            {
                int mid = (fromPos + toPos) / 2;

                MergeSortCore(list, comparer, fromPos, mid);
                MergeSortCore(list, comparer, mid + 1, toPos);

                int endLow = mid;
                int startHigh = mid + 1;

                while (fromPos <= endLow & startHigh <= toPos)
                {
                    if (comparer.Compare(list[fromPos], list[startHigh]) < 0)
                    {
                        fromPos++;
                    }
                    else
                    {
                        T temp = list[startHigh];
                        for (int index = startHigh - 1; index >= fromPos; index--)
                        {
                            list[index + 1] = list[index];
                        }

                        list[fromPos] = temp;
                        fromPos++;
                        endLow++;
                        startHigh++;
                    }
                }
            }
        }
    }
}
