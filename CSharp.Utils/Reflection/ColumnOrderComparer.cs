using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace CSharp.Utils.Reflection
{
    public sealed class ColumnOrderComparer : IComparer<PropertyInfo>
    {
        #region Static Fields

        private static readonly ColumnOrderComparer InstanceObject = new ColumnOrderComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        private ColumnOrderComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static ColumnOrderComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public static List<PropertyInfo> FilterPropertiesWithPropertyOrderAttribute(IEnumerable<PropertyInfo> properties)
        {
            var list = new List<PropertyInfo>();
            foreach (PropertyInfo property in properties)
            {
                var attribute = property.GetCustomAttribute<DisplayAttribute>(false);
                if (attribute != null)
                {
                    list.Add(property);
                }
            }

            return list;
        }

        public static void RemovePropertiesWithoutPropertyOrderAttribute(IList<PropertyInfo> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var attribute = list[i].GetCustomAttribute<DisplayAttribute>(false);
                if (attribute == null)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        public int Compare(PropertyInfo x, PropertyInfo y)
        {
            int xOrder = GetOrder(x);
            int yOrder = GetOrder(y);
            return xOrder.CompareTo(yOrder);
        }

        private static int GetOrder(PropertyInfo property)
        {
            var columnAttribute = (ColumnAttribute)Attribute.GetCustomAttribute(property, typeof(ColumnAttribute));
            if (columnAttribute != null)
            {
                return columnAttribute.Order;
            }
            
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(property, typeof(DisplayAttribute));
            if (displayAttribute != null)
            {
                return displayAttribute.Order;
            }
            
            return int.MaxValue;
        }

        #endregion Public Methods and Operators
    }
}
