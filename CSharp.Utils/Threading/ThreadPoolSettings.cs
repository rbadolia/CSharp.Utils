using System.Threading;

namespace CSharp.Utils.Threading
{
    public class ThreadLimits
    {
        #region Public Properties

        public int? CompletionPortThreads { get; set; }

        public int? WorkerThreads { get; set; }

        #endregion Public Properties
    }

    public sealed class ThreadPoolSettings
    {
        #region Static Fields

        private static readonly ThreadPoolSettings InstanceObject = new ThreadPoolSettings();

        #endregion Static Fields

        #region Constructors and Finalizers

        private ThreadPoolSettings()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static ThreadPoolSettings Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public ThreadLimits MaxThreads { get; set; }

        public ThreadLimits MinThreads { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Apply()
        {
            if (this.MinThreads != null && (this.MinThreads.WorkerThreads != null || this.MinThreads.CompletionPortThreads != null))
            {
                int minWorkerThreads;
                int minCompletionPortThreads;
                ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
                ThreadPool.SetMinThreads(this.MinThreads.WorkerThreads ?? minWorkerThreads, this.MinThreads.CompletionPortThreads ?? minCompletionPortThreads);
            }

            if (this.MaxThreads != null && (this.MaxThreads.WorkerThreads != null || this.MaxThreads.CompletionPortThreads != null))
            {
                int maxWorkerThreads;
                int maxCompletionPortThreads;
                ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
                ThreadPool.SetMaxThreads(this.MaxThreads.WorkerThreads ?? maxWorkerThreads, this.MaxThreads.CompletionPortThreads ?? maxCompletionPortThreads);
            }
        }

        #endregion Public Methods and Operators
    }
}
