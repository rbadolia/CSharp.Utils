using System;
using System.Collections.Generic;

namespace CSharp.Utils.Reflection.Internal
{
    internal static class IntelligentCopyHelper
    {
        #region Public Methods and Operators

        public static KeyValuePair<Type, Type>? CheckListTypes(Type t1, Type t2)
        {
            Type lt1 = getListGenericType(t1, true);
            if (lt1 != null)
            {
                Type lt2 = getListGenericType(t2, false);
                if (lt2 != null)
                {
                    return new KeyValuePair<Type, Type>(lt1, lt2);
                }
            }

            return null;
        }

        [CautionUsedByReflection]
        public static T GetNonNullableValue<T>(T? value) where T : struct
        {
            return value == null ? default(T) : value.Value;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static Type getListGenericType(Type t, bool acceptEnumerable)
        {
            if (t.IsGenericType)
            {
                Type acceptableType = acceptEnumerable ? typeof(IEnumerable<>) : typeof(IList<>);
                Type[] generics = t.GetGenericArguments();
                Type genericType = acceptableType.MakeGenericType(generics);
                if (genericType.IsAssignableFrom(t))
                {
                    return generics[0];
                }
            }

            return null;
        }

        #endregion Methods
    }
}
