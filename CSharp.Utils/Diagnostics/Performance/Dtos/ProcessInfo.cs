using System;

namespace CSharp.Utils.Diagnostics.Performance.Dtos
{
    [Serializable]
    public class ProcessInfo
    {
        #region Constructors and Finalizers

        public ProcessInfo()
        {
            this.ProcessId = ProcessHelper.CurrentProcess.Id;
            this.MachineName = GlobalSettings.Instance.MachineName;
            this.ApplicationName = GlobalSettings.Instance.ApplicationName;
            this.Environment = GlobalSettings.Instance.Environment;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string ApplicationName { get; set; }

        public string Environment { get; set; }

        public string MachineName { get; set; }

        public int ProcessId { get; set; }

        #endregion Public Properties
    }
}
