using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace CSharp.Utils.Data.Helpers
{
    public static class OleDbHelper
    {
        #region Constants

        public const string Excel08ConnectionStringFormatWithHeader = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};Extended Properties=\"Excel 8.0;HDR=YES; IMEX=1\"";

        public const string Excel08ConnectionStringFormatWithoutHeader = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};Extended Properties=\"Excel 8.0;HDR=NO; IMEX=1\"";

        public const string Excel12ConnectionStringFormatWithHeader = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES; IMEX=1\"";

        public const string Excel12ConnectionStringFormatWithoutHeader = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=NO; IMEX=1\"";

        #endregion Constants

        #region Public Methods and Operators

        public static List<DataTable> GetCompleteData(string fileName, string connectionStringFormat = Excel12ConnectionStringFormatWithHeader)
        {
            using (OleDbConnection connection = OpenConnection(fileName, connectionStringFormat))
            {
                List<string> sheetNames = GetTableNames(connection);
                var tables = new List<DataTable>();
                foreach (string sheetName in sheetNames)
                {
                    DataTable table = GetTableData(connection, sheetName);
                    tables.Add(table);
                }

                return tables;
            }
        }

        public static DataTable GetTableData(string fileName, string tableName, string connectionStringFormat = Excel12ConnectionStringFormatWithHeader)
        {
            using (OleDbConnection connection = OpenConnection(fileName, connectionStringFormat))
            {
                return GetTableData(connection, tableName);
            }
        }

        public static DataTable GetTableData(OleDbConnection connection, string tableName)
        {
            var dataAdapter = new OleDbDataAdapter("SELECT * FROM [" + tableName + "]", connection);
            var dataTable = new DataTable(tableName);
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public static List<string> GetTableNames(string fileName, string connectionStringFormat = Excel12ConnectionStringFormatWithHeader)
        {
            using (OleDbConnection connection = OpenConnection(fileName, connectionStringFormat))
            {
                return GetTableNames(connection);
            }
        }

        public static List<string> GetTableNames(OleDbConnection connection)
        {
            DataTable sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (sheets != null)
            {
                int columnIndex = sheets.Columns.IndexOf("TABLE_NAME");
                var sheetNames = new List<string>();
                foreach (DataRow row in sheets.Rows)
                {
                    string sheetName = row[columnIndex].ToString();
                    if (!sheetName.StartsWith("_xlnm#"))
                    {
                        sheetNames.Add(sheetName);
                    }
                }

                return sheetNames;
            }

            return new List<string>();
        }

        public static OleDbConnection OpenConnection(string fileName, string connectionStringFormat = Excel12ConnectionStringFormatWithHeader)
        {
            string connectionString = string.Format(connectionStringFormat, fileName);
            var connection = new OleDbConnection(connectionString);
            connection.Open();
            return connection;
        }

        #endregion Public Methods and Operators
    }
}
