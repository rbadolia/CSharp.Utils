using System.Collections.Generic;
using System.Data;
using System.Xml;
using CSharp.Utils.Configuration;
using CSharp.Utils.Data.Helpers;
using CSharp.Utils.Reflection;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Data
{
    public class FieldValuesMappingRepository : IConfigurable
    {
        #region Fields

        private readonly Dictionary<string, Dictionary<string, string>> _dictionary = new Dictionary<string, Dictionary<string, string>>();

        #endregion Fields

        #region Constructors and Finalizers

        public FieldValuesMappingRepository()
        {
        }

        public FieldValuesMappingRepository(bool isFieldNameCaseSensitive, bool isFieldValueCaseSensitive)
        {
            this.IsFieldNameCaseSensitive = isFieldNameCaseSensitive;
            this.IsFieldValueCaseSensitive = isFieldValueCaseSensitive;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public bool IsFieldNameCaseSensitive { get; private set; }

        public bool IsFieldValueCaseSensitive { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Configure(XmlNode configurationNode, IObjectInstantiator instantiator = null)
        {
            var mappings = new List<FieldValueMapping>();
            ComponentBuilder.PopulateList(mappings, configurationNode.ChildNodes[0], instantiator);
            this.PopulateMappings(mappings);
        }

        public string GetMappedValue(string fieldName, string fieldValue)
        {
            Guard.ArgumentNotNullOrEmpty(fieldName, "fieldName");
            Guard.ArgumentNotNull(fieldValue, "fieldValue");
            if (!this.IsFieldNameCaseSensitive)
            {
                fieldName = fieldName.ToUpper();
            }

            Dictionary<string, string> innerDictionary;
            if (!this._dictionary.TryGetValue(fieldName, out innerDictionary))
            {
                return null;
            }

            string mappedValue;
            if (!this.IsFieldValueCaseSensitive)
            {
                fieldValue = fieldValue.ToUpper();
            }

            innerDictionary.TryGetValue(fieldValue, out mappedValue);
            return mappedValue;
        }

        public void PopulateMappings(IEnumerable<FieldValueMapping> mappings)
        {
            foreach (FieldValueMapping mapping in mappings)
            {
                string fieldName = this.IsFieldNameCaseSensitive ? mapping.FieldName : mapping.FieldName.ToUpper();
                string fieldValue = this.IsFieldValueCaseSensitive ? mapping.FieldValue : mapping.FieldValue.ToUpper();

                Dictionary<string, string> innerDictionary;
                if (!this._dictionary.TryGetValue(fieldName, out innerDictionary))
                {
                    innerDictionary = new Dictionary<string, string>();
                    this._dictionary.Add(fieldName, innerDictionary);
                }

                innerDictionary.Add(fieldValue, mapping.MappedValue);
            }
        }

        public void PopulateMappings(DataTable table)
        {
            List<FieldValueMapping> mappings = table.ExportAsListOfObjects<FieldValueMapping>(false);
            this.PopulateMappings(mappings);
        }

        #endregion Public Methods and Operators
    }
}
