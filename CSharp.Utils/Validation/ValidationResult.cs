namespace CSharp.Utils.Validation
{
    public sealed class ValidationResult
    {
        #region Public Properties

        public string ErrorMessage { get; set; }

        public int? ItemSequenceNumber { get; set; }

        public string ObjectPath { get; set; }

        public string PropertyName { get; set; }

        public ValidationStatus Status { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public override string ToString()
        {
            if (this.Status == ValidationStatus.Success)
            {
                return "Success";
            }

            string propertyPath = null;
            if (this.ItemSequenceNumber == null)
            {
                propertyPath = this.ObjectPath == null ? this.PropertyName : string.Format("{0}.{1}", this.ObjectPath, this.PropertyName);
            }
            else
            {
                propertyPath = string.Format("{0}[{1}].{2}", this.ObjectPath, this.ItemSequenceNumber.Value, this.PropertyName);
            }

            return string.Format("Status={0}, PropertyPath={1}, ErrorMessage={2}", this.Status, propertyPath, this.ErrorMessage);
        }

        #endregion Public Methods and Operators
    }
}
