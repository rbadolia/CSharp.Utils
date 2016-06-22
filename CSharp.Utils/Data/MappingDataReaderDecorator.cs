using System;
using System.Data;
using System.Globalization;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Data
{
    public class MappingDataReaderDecorator : DataReaderBase, IDataReader
    {
        #region Fields

        private readonly FieldValuesMappingRepository _repository;

        #endregion Fields

        #region Constructors and Finalizers

        public MappingDataReaderDecorator(IDataReader adaptedObject, FieldValuesMappingRepository repository)
            : base(adaptedObject)
        {
            this._repository = repository;
        }

        #endregion Constructors and Finalizers

        #region Properties

        protected FieldValuesMappingRepository mappingsEntityService
        {
            get
            {
                return this._repository;
            }
        }

        #endregion Properties

        #region Public Indexers

        public object this[string name]
        {
            get
            {
                int ordinal = this.GetOrdinal(name);
                return this[ordinal];
            }
        }

        public object this[int i]
        {
            get
            {
                return this.GetValue(i);
            }
        }

        #endregion Public Indexers

        #region Public Methods and Operators

        public bool GetBoolean(int i)
        {
            bool originalValue = this.AdaptedObject.GetBoolean(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString());
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Convert.ToBoolean(mappedValueString);
        }

        public char GetChar(int i)
        {
            char originalValue = this.AdaptedObject.GetChar(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null || mappedValueString.Length != 1)
            {
                return originalValue;
            }

            return mappedValueString[0];
        }

        public DateTime GetDateTime(int i)
        {
            DateTime originalValue = this.AdaptedObject.GetDateTime(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Convert.ToDateTime(mappedValueString);
        }

        public decimal GetDecimal(int i)
        {
            decimal originalValue = this.AdaptedObject.GetDecimal(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Convert.ToDecimal(mappedValueString);
        }

        public double GetDouble(int i)
        {
            double originalValue = this.AdaptedObject.GetDouble(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Convert.ToDouble(mappedValueString);
        }

        public float GetFloat(int i)
        {
            float originalValue = this.AdaptedObject.GetFloat(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return float.Parse(mappedValueString);
        }

        public Guid GetGuid(int i)
        {
            Guid originalValue = this.AdaptedObject.GetGuid(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString());
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Guid.Parse(mappedValueString);
        }

        public short GetInt16(int i)
        {
            short originalValue = this.AdaptedObject.GetInt16(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Convert.ToInt16(mappedValueString);
        }

        public int GetInt32(int i)
        {
            int originalValue = this.AdaptedObject.GetInt32(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Convert.ToInt32(mappedValueString);
        }

        public long GetInt64(int i)
        {
            long originalValue = this.AdaptedObject.GetInt64(i);
            string fieldName = this.GetName(i);
            string mappedValueString = this._repository.GetMappedValue(fieldName, originalValue.ToString(CultureInfo.InvariantCulture));
            if (mappedValueString == null)
            {
                return originalValue;
            }

            return Convert.ToInt64(mappedValueString);
        }

        public string GetString(int i)
        {
            string originalValue = this.AdaptedObject.GetString(i);
            string fieldName = this.GetName(i);
            return this.mapString(fieldName, originalValue);
        }

        public object GetValue(int i)
        {
            object originalValue = this.AdaptedObject.GetValue(i);
            if (originalValue == null || this.IsDBNull(i))
            {
                return originalValue;
            }

            string fieldName = this.GetName(i);
            Type type = originalValue.GetType();
            if (type == typeof(string))
            {
                return this.mapString(fieldName, (string)originalValue);
            }

            string mappedValueString = this.mapString(fieldName, originalValue.ToString());
            if (mappedValueString == null)
            {
                return originalValue;
            }

            object convertedValue = ReflectionHelper.ConvertUsingConvertTo(type, mappedValueString);
            return convertedValue ?? originalValue;
        }

        public int GetValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = this.GetValue(i);
            }

            return values.Length;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected string mapString(string fieldName, string originalValue)
        {
            string mappedValue = this._repository.GetMappedValue(fieldName, originalValue);
            if (mappedValue == null)
            {
                return originalValue;
            }

            return mappedValue;
        }

        #endregion Methods
    }
}
