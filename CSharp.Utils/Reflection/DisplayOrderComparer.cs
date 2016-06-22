// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisplayOrderComparer.cs" company="">
//   
// </copyright>
// <summary>
//   The display order comparer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#region License
// // Copyright 2014 Eurofins Scientific Ltd, Ireland
// // Usage reserved to Eurofins Global Franchise Model subscribers.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CSharp.Utils.Reflection
{
    /// <summary>
    /// The display order comparer.
    /// </summary>
    public sealed class DisplayOrderComparer : IComparer<PropertyInfo>
    {
        #region Static Fields

        /// <summary>
        /// The instance object.
        /// </summary>
        private static readonly DisplayOrderComparer InstanceObject = new DisplayOrderComparer();

        #endregion Static Fields

        #region Constructors and Finalizers

        /// <summary>
        /// Prevents a default instance of the <see cref="DisplayOrderComparer"/> class from being created.
        /// </summary>
        private DisplayOrderComparer()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static DisplayOrderComparer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        /// <summary>
        /// The filter properties with property order attribute.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
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

        /// <summary>
        /// The remove properties without property order attribute.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
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

        /// <summary>
        /// The compare.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Compare(PropertyInfo x, PropertyInfo y)
        {
            var xAttribute = x.GetCustomAttribute<DisplayAttribute>(false);
            var yAttribute = y.GetCustomAttribute<DisplayAttribute>(false);
            if (xAttribute != null && yAttribute != null)
            {
                return xAttribute.Order.CompareTo(yAttribute.Order);
            }

            if (xAttribute == null && yAttribute == null)
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }

            return xAttribute == null ? 1 : -1;
        }

        #endregion Public Methods and Operators
    }
}
