namespace CSharp.Utils.Data.Dynamic
{
    public class ClauseCondition
    {
        #region Constructors and Finalizers

        public ClauseCondition()
        {
        }

        public ClauseCondition(string parameterName, string operaterStr, object parameterValue)
        {
            this.ParameterName = parameterName;
            this.Operator = operaterStr;
            this.ParameterValue = parameterValue;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string Operator { get; set; }

        public string ParameterName { get; set; }

        public object ParameterValue { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static ClauseCondition CreateNew(string parameterName, string operaterStr, object parameterValue)
        {
            return new ClauseCondition(parameterName, operaterStr, parameterValue);
        }

        #endregion Public Methods and Operators
    }
}
