namespace CSharp.Utils.Diagnostics.Performance
{
    public class CustomCounterInfo
    {
        #region Constructors and Finalizers

        public CustomCounterInfo()
        {
            this.IsCurrentProcess = true;
            this.IsRaw = false;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CategoryName { get; set; }

        public string CounterName { get; set; }

        public string InstanceName { get; set; }

        public bool IsCurrentProcess { get; set; }

        public bool IsRaw { get; set; }

        #endregion Public Properties
    }
}
