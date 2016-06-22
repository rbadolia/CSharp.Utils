using CSharp.Utils.Contracts;

namespace CSharp.Utils.Diagnostics
{
    public class MemoryDumpScheduleInfo
    {
        #region Constructors and Finalizers

        public MemoryDumpScheduleInfo()
        {
            this.IsContinuous = true;
            this.IntervalToTryInSeconds = 30;
            this.CaptureDumpOnEveryRetryFailed = false;
            this.CaptureDumpIfRetryIsSuccess = false;
            this.MaximumNumberOfContinuousFailuresToDump = 1;
            this.DumpType = MemoryDumpTypes.MiniDumpWithFullMemory;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public bool CaptureDumpIfRetryIsSuccess { get; set; }

        public bool CaptureDumpOnEveryRetryFailed { get; set; }

        public string DumpDirectoryPath { get; set; }

        public MemoryDumpTypes DumpType { get; set; }

        public long IntervalToTryInSeconds { get; set; }

        public bool IsContinuous { get; set; }

        public int MaximumNumberOfContinuousFailuresToDump { get; set; }

        public string ProcessIdOrName { get; set; }

        public IConditionStrategy Strategy { get; set; }

        #endregion Public Properties
    }
}
