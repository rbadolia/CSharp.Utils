using System;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CSharp.Utils.Data.Helpers
{
    public static class DataHelper
    {
        #region Public Methods and Operators

        public static T GetValueFromDataRecord<T>(IDataRecord record, int index)
        {
            object o = record[index];
            return o == DBNull.Value || o == null ? default(T) : (T)o;
        }

        public static DataSet ReadDataSetFromXml(string xml)
        {
            return ReadFromXml<DataSet>(xml);
        }

        public static DataTable ReadDataTableFromXml(string xml)
        {
            return ReadFromXml<DataTable>(xml);
        }

        public static T ReadFromXml<T>(string xml) where T : IXmlSerializable, new()
        {
            using (var reader = new StringReader(xml))
            {
                using (var xReader = new XmlTextReader(reader))
                {
                    var obj = new T();
                    obj.ReadXml(xReader);
                    return obj;
                }
            }
        }

        #endregion Public Methods and Operators
    }
}
