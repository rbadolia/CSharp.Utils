using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Data.Helpers
{
    public static class DataTableHelper
    {
        #region Public Methods and Operators

        public static void AddToDataTable<T>(this IEnumerable<T> enumerable, DataTable table) where T : class
        {
            IEnumerable<DataRow> rows = DynamicToDataReaderDataTableHelper<T>.ExportAsDataRows(enumerable);
            foreach (DataRow row in rows)
            {
                table.Rows.Add(row);
            }
        }

        public static void ChangeColumnValues(this DataTable dataTable, string columnName, string existingValue, string newValue, bool ignoreCase)
        {
            if (!dataTable.Columns.Contains(columnName))
            {
                return;
            }

            DataColumn column = dataTable.Columns[columnName];
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                object o = dataTable.Rows[i][column];
                if (o != null && o != DBNull.Value)
                {
                    string original = o.ToString();
                    if (string.Compare(original, existingValue, ignoreCase) == 0)
                    {
                        dataTable.Rows[i][column] = newValue;
                    }
                }
            }
        }

        public static List<T> ConvertDataTableToObject<T>(DataTable dataTable, bool removeEmptyObjects, string root = "DocumentElement")
        {
            dataTable.TableName = typeof(T).Name;
            List<T> list;
            using (var stream = new MemoryStream())
            {
                dataTable.WriteXml(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var rootAttribute = new XmlRootAttribute(root);
                var serializer = new XmlSerializer(typeof(List<T>), rootAttribute);
                list = (List<T>)serializer.Deserialize(stream);
            }

            if (removeEmptyObjects)
            {
                DynamicObjectEmptyCheckHelper<T>.RemoveEmptyElements(list, true);
            }

            return list;
        }

        public static IEnumerable<DataRow> ExportAsDataRows<T>(this IEnumerable<T> enumerable) where T : class
        {
            return DynamicToDataReaderDataTableHelper<T>.ExportAsDataRows(enumerable);
        }

        public static DataTable ExportAsDataTable<T>(this IEnumerable<T> enumerable, string dataTableName, DataSet dataSetOfDataTable = null) where T : class
        {
            DataTable dataTable = DynamicToDataReaderDataTableHelper<T>.ExportAsDataTable(enumerable);
            dataTable.TableName = dataTableName;
            if (dataSetOfDataTable != null)
            {
                dataSetOfDataTable.Tables.Add(dataTable);
            }

            return dataTable;
        }

        public static List<T> ExportAsListOfObjects<T>(this DataTable dataTable, bool removeEmptyObjects, string root = "DocumentElement")
        {
            dataTable.TableName = typeof(T).Name;
            List<T> list = dataTable.Rows.Count == 0 ? new List<T>() : ConvertDataTableToObjectPrivate(dataTable, typeof(List<T>), root) as List<T>;
            if (removeEmptyObjects)
            {
                DynamicObjectEmptyCheckHelper<T>.RemoveEmptyElements(list, true);
            }

            return list;
        }

        public static DataTable GetDataTableByName(this IEnumerable<DataTable> tables, string tableName, bool ignoreCase = true)
        {
            foreach (DataTable table in tables)
            {
                if (string.Compare(table.TableName, tableName, ignoreCase) == 0)
                {
                    return table;
                }
            }

            return null;
        }

        public static void Merge(this DataTable to, DataTable from)
        {
            for (int i = 0; i < from.Rows.Count; i++)
            {
                to.Rows.Add(from.Rows[i].ItemArray);
            }
        }

        public static void ReplaceStrings(this DataTable table, IList<KeyValuePair<string, string>> pairs, bool ignoreCase = false, bool trimStrings = false)
        {
            if (trimStrings)
            {
                TrimStrings(table);
            }

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].DataType == typeof(string))
                {
                    foreach (var kvp in pairs)
                    {
                        for (int j = 0; j < table.Rows.Count; j++)
                        {
                            object o = table.Rows[j][i];
                            if (o != null && o != DBNull.Value)
                            {
                                var originalvalue = (string)o;
                                table.Rows[j][i] = StringHelper.Replace(originalvalue, kvp.Key, kvp.Value, ignoreCase);
                            }
                        }
                    }
                }
            }
        }

        public static DataTable TransposeDataTable(DataTable table, string rowHeaderColumnName = "Header")
        {
            var dt = new DataTable();
            dt.Columns.Add(rowHeaderColumnName, typeof(string));

            for (int i = 0; i < table.Rows.Count; i++)
            {
                dt.Columns.Add("R" + (i + 1).ToString(CultureInfo.InvariantCulture));
            }

            var data = new object[table.Rows.Count + 1];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                data[0] = table.Columns[i].ColumnName;
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    data[i + 1] = table.Rows[j][j];
                }

                dt.Rows.Add(data);
            }

            return dt;
        }

        public static void TrimColumnNames(this DataTable table)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                table.Columns[i].ColumnName = table.Columns[i].ColumnName.Trim();
            }
        }

        public static void TrimStrings(this DataTable table)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].DataType == typeof(string))
                {
                    for (int j = 0; j < table.Rows.Count; j++)
                    {
                        object o = table.Rows[j][i];
                        if (o != null && o != DBNull.Value)
                        {
                            table.Rows[j][i] = ((string)o).Trim();
                        }
                    }
                }
            }
        }

        public static void UpdateColumnNamesWithFirstRowValuesAndRemoveTheRow(this DataTable dataTable)
        {
            if (dataTable.Rows.Count > 0)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    object cellValue = dataTable.Rows[0][i];
                    if (cellValue != null && cellValue != DBNull.Value)
                    {
                        dataTable.Columns[i].ColumnName = cellValue.ToString();
                    }
                }

                dataTable.Rows.RemoveAt(0);
            }
        }

        public static void UpdateColumnNamesWithFirstRowValuesAndRemoveTheRow(this IEnumerable<DataTable> dataTables)
        {
            foreach (DataTable table in dataTables)
            {
                UpdateColumnNamesWithFirstRowValuesAndRemoveTheRow(table);
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        private static object ConvertDataTableToObjectPrivate(DataTable dataTable, Type objectType, string root = "DocumentElement")
        {
            using (var stream = new MemoryStream())
            {
                dataTable.WriteXml(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var rootAttribute = new XmlRootAttribute(root);
                var serializer = new XmlSerializer(objectType, rootAttribute);
                return serializer.Deserialize(stream);
            }
        }

        #endregion Methods
    }
}
