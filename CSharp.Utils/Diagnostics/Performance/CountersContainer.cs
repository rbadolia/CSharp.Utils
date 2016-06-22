namespace CSharp.Utils.Diagnostics.Performance
{
    public class ProcessSettingsInfo
    {
        #region Constructors and Finalizers

        public ProcessSettingsInfo()
        {
            this.ShouldPublish = true;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public int? PreviousProcessId { get; set; }

        public int ProcessId { get; set; }

        public bool ShouldPublish { get; set; }

        #endregion Public Properties
    }
}
