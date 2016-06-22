using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Diagnostics
{
    public class MemoryDumpScheduler : AbstractDisposable
    {
        #region Fields

        private readonly Dictionary<MemoryDumpScheduleInfo, ScheduleInfo> infos = new Dictionary<MemoryDumpScheduleInfo, ScheduleInfo>();

        private readonly IntervalMultiTask task = new IntervalMultiTask(true);

        #endregion Fields

        #region Public Methods and Operators

        public void AddDumpInfo(MemoryDumpScheduleInfo info)
        {
            ActionDelegate a = delegate
                {
                    ScheduleInfo si = this.infos[info];
                    if (!info.Strategy.ExecuteCondition())
                    {
                        si.FailedCount++;
                        if (si.FailedCount > 1 && info.CaptureDumpOnEveryRetryFailed)
                        {
                            this.dump(info);
                            return;
                        }

                        if (info.MaximumNumberOfContinuousFailuresToDump <= si.FailedCount)
                        {
                            this.dump(info);
                            if (!info.IsContinuous)
                            {
                                this.task.RemoveAction(si.Action, null);
                                this.infos.Remove(info);
                            }
                        }
                    }
                    else
                    {
                        if (si.FailedCount != 0 && info.CaptureDumpIfRetryIsSuccess)
                        {
                            this.dump(info);
                        }

                        si.FailedCount = 0;
                    }
                };
            var sInfo = new ScheduleInfo { Action = a };
            this.infos.Add(info, sInfo);
            this.task.AddAction(a, info.IntervalToTryInSeconds * 1000, null);
        }

        public void RemoveDumpInfo(MemoryDumpScheduleInfo info)
        {
            this.infos.Remove(info);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.task.Dispose();
        }

        private void dump(MemoryDumpScheduleInfo info)
        {
            string directoryPath = Path.GetFullPath(info.DumpDirectoryPath);
            string dateTime = GlobalSettings.Instance.CurrentDateTime.ToString(CultureInfo.InvariantCulture);
            dateTime.MutableReplace('/', '_');
            dateTime.MutableReplace(':', '_');
            string fileName = string.Format(@"{0}\{1}_{2}.dmp", directoryPath, info.ProcessIdOrName, dateTime);
            MemoryDumpHelper.CreateMemoryDump(info.DumpType, info.ProcessIdOrName, fileName);
        }

        #endregion Methods

        private sealed class ScheduleInfo
        {
            #region Public Properties

            public ActionDelegate Action { get; set; }

            public int FailedCount { get; set; }

            #endregion Public Properties
        }
    }
}
