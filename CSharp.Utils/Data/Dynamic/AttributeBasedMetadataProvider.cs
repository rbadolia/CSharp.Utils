using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using CSharp.Utils.Data.Helpers;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Data.Dynamic
{
    public class AttributeBasedMetadataProvider : IMetadataProvider
    {
        #region Public Methods and Operators

        public static IDbTableMetadata GetMetaDataForType(Type entityType)
        {
            string tableName = entityType.Name;
            var tableAttribute = entityType.GetCustomAttribute<TableAttribute>(false);

            if (tableAttribute != null)
            {
                tableName = tableAttribute.Name;
            }

            List<PropertyInfo> properties = ReflectionHelper.GetPublicProperties(entityType, false);
            var mappings = new List<KeyValuePair<PropertyInfo, DbColumnAttribute>>();
            foreach (PropertyInfo property in properties)
            {
                var na = property.GetCustomAttribute<NotDbColumnAttribute>(true);
                if (na != null)
                {
                    continue;
                }

                var attrib = property.GetCustomAttribute<DbColumnAttribute>(true);
                if (attrib != null)
                {
                    attrib.Property = property;
                    mappings.Add(new KeyValuePair<PropertyInfo, DbColumnAttribute>(property, attrib));
                }
                else
                {
                    bool isKey = property.IsDefined(typeof(KeyAttribute), true);
                    attrib = new DbColumnAttribute(property.Name, isKey) { Property = property, AccessType = (property.CanRead && property.CanWrite) ? DbColumnAccessTypes.ReadWrite : (property.CanRead ? DbColumnAccessTypes.Write : DbColumnAccessTypes.Read), IsNullable = true };
                    DbType dbType;
                    if (property.PropertyType.IsEnum)
                    {
                        dbType = DbType.Int32;
                        attrib.IsNullable = false;
                    }
                    else
                    {
                        if (!DbTypeMappingHelper.TryGetDbType(property.PropertyType, out dbType))
                        {
                            continue;
                        }
                    }

                    var requiredAttrib = property.GetCustomAttribute<RequiredAttribute>(true);
                    if (requiredAttrib != null)
                    {
                        attrib.IsNullable = false;
                    }
                    else
                    {
                        if (property.PropertyType.IsValueType)
                        {
                            attrib.IsNullable = property.PropertyType.IsGenericType;
                        }
                    }

                    attrib.ColumnDbType = dbType;
                    var maxLengthAttrib = property.GetCustomAttribute<MaxLengthAttribute>(true);
                    if (maxLengthAttrib != null)
                    {
                        attrib.MaxLength = maxLengthAttrib.Length;
                    }

                    mappings.Add(new KeyValuePair<PropertyInfo, DbColumnAttribute>(property, attrib));
                }
            }

            var columnsMetaData = new List<IDbColumnMetadata>();
            foreach (var kvp in mappings)
            {
                columnsMetaData.Add(kvp.Value);
            }

            var tableMetaData = new DbTableMetadata(tableName, columnsMetaData);
            return tableMetaData;
        }

        public IDbTableMetadata GetMetaData(Type entityType)
        {
            return GetMetaDataForType(entityType);
        }

        #endregion Public Methods and Operators
    }
}
