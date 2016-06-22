namespace CSharp.Utils.Diagnostics.Performance
{
    public class PropertyMetaData
    {
        #region Constructors and Finalizers

        public PropertyMetaData(string propertyName, string groupName, string counterName)
            : this()
        {
            this.PropertyName = propertyName;
            this.GroupName = groupName;
            this.CounterName = counterName;
        }

        public PropertyMetaData()
        {
            this.IsValidForWindowsPerfCounter = true;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CounterName { get; set; }

        public string GroupName { get; set; }

        public bool IsValidForWindowsPerfCounter { get; set; }

        public string PropertyName { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static PropertyMetaData CreateNew(string propertyName, string groupName, string counterName)
        {
            return new PropertyMetaData(propertyName, groupName, counterName);
        }

        #endregion Public Methods and Operators
    }
}
