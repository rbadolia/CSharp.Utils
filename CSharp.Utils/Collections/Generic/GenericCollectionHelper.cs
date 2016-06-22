using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Collections.Generic
{
    public static class GenericCollectionHelper
    {
        #region Public Methods and Operators

        public static bool Equals(IList list1, IList list2)
        {
            if (list1 == null && list2 == null)
            {
                return true;
            }

            if (list1 == null || list2 == null)
            {
                return false;
            }

            if (ReferenceEquals(list1, list2))
            {
                return true;
            }

            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] == null)
                {
                    if (list2[i] != null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!list1[i].Equals(list2[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static void ExportToStringSeparatedFile<T>(this IEnumerable<T> enumerable, string fileName, Encoding encoding = null, string delimiter = "\t")
        {
            StreamWriter writer = encoding == null ? new StreamWriter(fileName) : new StreamWriter(fileName, false, encoding);
            using (writer)
            {
                DynamicStringSeparatedExportHelper<T>.WriteToTextWriter(enumerable, writer, delimiter, true);
            }
        }

        public static IEnumerable<T> FilterByType<T>(IEnumerable enumerable)
        {
            foreach (object v in enumerable)
            {
                if (v != null)
                {
                    if (v is T)
                    {
                        var t = (T)v;
                        yield return t;
                    }
                }
            }
        }

        #endregion Public Methods and Operators
    }
}
