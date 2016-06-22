using CSharp.Utils.Collections.Concurrent;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class LockFactory : AbstractDisposable
    {
        #region Fields

        private readonly object syncLock = new object();

        private readonly CompositeDictionary<string, object> locks = new CompositeDictionary<string, object>(false);

        #endregion Fields

        #region Public Methods and Operators

        public object GetLock(string machineName, string processName, string environment)
        {
            lock (this.syncLock)
            {
                object o = this.locks.Get(machineName, processName, environment);
                if (o == null)
                {
                    o = new object();
                    this.locks.AddOrUpdate(o, machineName, processName, environment);
                }

                return o;
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.locks.Dispose();
        }

        #endregion Methods
    }
}
