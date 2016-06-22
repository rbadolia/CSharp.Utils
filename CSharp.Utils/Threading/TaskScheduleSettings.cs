using System.Collections.Generic;

namespace CSharp.Utils.Threading
{
    public sealed class TaskScheduleSettings
    {
        #region Constructors and Finalizers

        public TaskScheduleSettings()
        {
            this.Timings = new List<Time>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public bool PerformTaskOnStart { get; set; }

        public List<Time> Timings { get; private set; }

        #endregion Public Properties
    }
}
