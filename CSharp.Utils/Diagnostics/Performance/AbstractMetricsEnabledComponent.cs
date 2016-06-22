namespace CSharp.Utils.Diagnostics.Performance
{
    public abstract class AbstractMetricsEnabledComponent<T> : AbstractInitializableAndDisposable
        where T : class
    {
        #region Fields

        private bool isAddedToPerfManager;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractMetricsEnabledComponent()
        {
            this.CounterStorageType = CounterStorageType.None;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public CounterStorageType CounterStorageType { get; set; }

        public string Name { get; set; }

        #endregion Public Properties

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.isAddedToPerfManager)
            {
                PerfCounterManager.Instance.RemoveInstance(this as T);
                this.isAddedToPerfManager = false;
            }
        }

        protected override void InitializeProtected()
        {
            if (this.CounterStorageType != CounterStorageType.None)
            {
                this.registerToPerfManager();
                this.isAddedToPerfManager = true;
            }
        }

        protected virtual void registerToPerfManager()
        {
            PerfCounterManager.Instance.AddInstance(this as T, this.Name, this.CounterStorageType);
        }

        #endregion Methods
    }
}
