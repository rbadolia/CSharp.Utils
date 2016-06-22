using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using CSharp.Utils.Data;

namespace CSharp.Utils.Web
{
    public static class HtmlExportHelper
    {
        #region Constants

        public const string CellStyle = "border-width: 1px;padding: 8px;border-style: solid;border-color: #666666;background-color: #ffffff;text-align: left;";

        public const string HeaderCellStyle = "border-width: 1px;padding: 8px;border-style: solid;border-color: #666666;background-color: #dedede;text-align: center;";

        public const string NumericCellStyle = "border-width: 1px;padding: 8px;border-style: solid;border-color: #666666;background-color: #ffffff;text-align: right;";

        public const string TableStyle = "font-family: verdana,arial,sans-serif;font-size:11px;color:#333333;border-width: 1px;border-color: #666666;border-collapse: collapse;";

        #endregion Constants

        #region Public Methods and Operators

        public static void ExportAsHtml(string fileName, IDataReader reader, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title, string tableTitle, Encoding encoding = null)
        {
            ExportAsHtml(fileName, new DataReaderDataTableAdapter(reader, tableTitle), tableStyle, headerCellStyle, numericCellStyle, cellStyle, title, tableTitle, encoding);
        }

        public static void ExportAsHtml(string fileName, DataTable table, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title, string tableTitle, Encoding encoding = null)
        {
            ExportAsHtml(fileName, new DataReaderDataTableAdapter(table), tableStyle, headerCellStyle, numericCellStyle, cellStyle, title, tableTitle, encoding);
        }

        public static void ExportAsHtml(string fileName, DataReaderDataTableAdapter reader, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title, string tableTitle, Encoding encoding = null)
        {
            StreamWriter writer = encoding == null ? new StreamWriter(fileName) : new StreamWriter(fileName, false, encoding);
            using (writer)
            {
                ExportAsHtml(writer, reader, tableStyle, headerCellStyle, numericCellStyle, cellStyle, title, tableTitle);
            }
        }

        public static void ExportAsHtml(TextWriter writer, DataReaderDataTableAdapter reader, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title, string tableTitle)
        {
            WriteStyledHtmlHeader(writer, tableStyle, headerCellStyle, numericCellStyle, cellStyle, title);
            WriteTableHeader(writer, reader, true, tableTitle);
            WriteRowsHtml(writer, reader, true);
            writer.Write("</tbody></table></body></html>");
        }

        public static void ExportAsHtml(TextWriter writer, DataReaderDataTableAdapter reader, string title, string tableTitle)
        {
            writer.Write("<html>\r\n<head>\r\n<title>");
            writer.Write(HttpUtility.HtmlEncode(title) + "</title>\r\n</head>\r\n");
            WriteTableHeader(writer, reader, false, title);
            WriteRowsHtml(writer, reader, false);
            writer.Write("</tbody></table></body></html>");
        }

        public static string ExportAsHtml(DataReaderDataTableAdapter reader, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title, string tableTitle)
        {
            using (var writer = new StringWriter())
            {
                ExportAsHtml(writer, reader, tableStyle, headerCellStyle, numericCellStyle, cellStyle, title, tableTitle);
                return writer.ToString();
            }
        }

        public static string ExportAsHtml(DataReaderDataTableAdapter reader, string title, string tableTitle)
        {
            using (var writer = new StringWriter())
            {
                ExportAsHtml(writer, reader, title, tableTitle);
                return writer.ToString();
            }
        }

        public static string ExportAsHtml(IDataReader reader, string title, string tableTitle)
        {
            return ExportAsHtml(new DataReaderDataTableAdapter(reader, tableTitle), title, tableTitle);
        }

        public static string ExportAsHtml(IDataReader reader, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title, string tableTitle)
        {
            return ExportAsHtml(new DataReaderDataTableAdapter(reader, tableTitle), tableStyle, headerCellStyle, numericCellStyle, cellStyle, title, tableTitle);
        }

        public static string ExportAsHtml(DataTable table, string title, string tableTitle)
        {
            return ExportAsHtml(new DataReaderDataTableAdapter(table), title, tableTitle);
        }

        public static string ExportAsHtml(DataTable table, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title, string tableTitle)
        {
            return ExportAsHtml(new DataReaderDataTableAdapter(table), tableStyle, headerCellStyle, numericCellStyle, cellStyle, title, tableTitle);
        }

        public static void WriteRowsHtml(TextWriter writer, DataReaderDataTableAdapter reader, bool isStyled)
        {
            while (reader.Read())
            {
                writer.Write("<tr>");
                bool[] numericStyles = null;
                if (isStyled)
                {
                    numericStyles = new bool[reader.ColumnCount];
                    for (int i = 0; i < reader.ColumnCount; i++)
                    {
                        Type columnType = reader.GetColumnType(i);
                        numericStyles[i] = columnType == typeof(long) || columnType == typeof(int) || columnType == typeof(short) || columnType == typeof(float) || columnType == typeof(double) || columnType == typeof(decimal);
                    }
                }

                for (int k = 0; k < reader.ColumnCount; k++)
                {
                    writer.Write(isStyled ? "<td class=\"td" + (numericStyles[k] ? "N" : "C") + "\">" : "<td>");
                    object value = reader.GetValue(k);
                    string objectValue = value == null ? string.Empty : value is DateTime ? ((DateTime)value).ConvertDateTimeToString() : HttpUtility.HtmlEncode(value.ToString()).Replace(Environment.NewLine, "<br/>");
                    writer.Write(objectValue);
                    writer.Write("</td>");
                }

                writer.Write("</tr>\r\n");
            }
        }

        public static void WriteStyledHtmlHeader(TextWriter writer, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle, string title)
        {
            writer.Write("<html>\r\n<head>\r\n");
            writer.Write("<title>" + HttpUtility.HtmlEncode(title) + "</title>\r\n");
            WriteStyles(writer, tableStyle, headerCellStyle, numericCellStyle, cellStyle);
            writer.Write("</head>\r\n<body>\r\n");
        }

        public static void WriteStyles(TextWriter writer, string tableStyle, string headerCellStyle, string numericCellStyle, string cellStyle)
        {
            writer.Write("<style type=\"text/css\">\r\n");
            writer.Write("table.tbl\r\n{\t" + tableStyle + "\r\n}\r\n");
            writer.Write("th.header\r\n{\r\n\t" + headerCellStyle + "\r\n}\r\n");
            writer.Write("td.tdN\r\n{\r\n\t" + numericCellStyle + "\r\n}\r\n");
            writer.Write("td.tdC\r\n{\r\n\t" + cellStyle + "\r\n}\r\n");
            writer.Write("</style>\r\n");
        }

        public static void WriteTableHeader(TextWriter writer, DataReaderDataTableAdapter reader, bool isStyled, string tableTitle, string tableTitleStyle = null)
        {
            writer.Write("<table cellspacing=\"0\"");
            if (isStyled)
            {
                writer.Write(" class=\"tbl\"");
            }

            writer.Write(">\r\n<thead>\r\n<tr>");
            if (!string.IsNullOrWhiteSpace(tableTitle))
            {
                writer.Write("<th style=\"" + tableTitleStyle + "\" colspan=\"" + reader.ColumnCount + "\">" + tableTitle + "</th></tr><tr>");
            }

            for (int i = 0; i < reader.ColumnCount; i++)
            {
                writer.Write(isStyled ? "<th class=\"header\">" : "<th>");
                writer.Write(reader.GetColumnName(i));
                writer.Write("</th>");
            }

            writer.Write("</tr>\r\n</thead>\r\n<tbody>\r\n");
        }

        #endregion Public Methods and Operators
    }
}
