namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class CounterPublisherLocal
    {
        #region Static Fields

        private static readonly CounterPublisherLocal InstanceObject = new CounterPublisherLocal();

        #endregion Static Fields

        #region Fields

        private bool enablePublishing;

        #endregion Fields

        #region Constructors and Finalizers

        private CounterPublisherLocal()
        {
            this.EnablePublishing = true;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static CounterPublisherLocal Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public bool EnablePublishing
        {
            get
            {
                return this.enablePublishing;
            }

            set
            {
                if (this.enablePublishing != value)
                {
                    if (this.enablePublishing)
                    {
                        PerfCounterManager.Instance.AfterCountersCaptured -= this.Instance_AfterCountersCaptured;
                    }
                    else
                    {
                        PerfCounterManager.Instance.AfterCountersCaptured += this.Instance_AfterCountersCaptured;
                    }

                    this.enablePublishing = value;
                }
            }
        }

        #endregion Public Properties

        #region Methods

        private void Instance_AfterCountersCaptured(object sender, CountersCapturedEventArgs e)
        {
            if (this.EnablePublishing)
            {
                CountersCacheManager.Instance.PublishCounters(e.Info);
            }
        }

        #endregion Methods
    }
}
