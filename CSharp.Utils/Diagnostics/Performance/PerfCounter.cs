using System;
using System.Diagnostics;

namespace CSharp.Utils.Diagnostics.Performance
{
    [Serializable]
    public sealed class PerfCounter
    {
        #region Fields

        [NonSerialized]
        private PerformanceCounter adaptedObject;

        private double _rawValue;

        #endregion Fields

        #region Public Properties

        public double RawValue
        {
            get
            {
                return this._rawValue;
            }

            set
            {
                this._rawValue = value;
                if (this.adaptedObject != null)
                {
                    this.adaptedObject.RawValue = (long)value;
                }
            }
        }

        public object Tag { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Dispose()
        {
            if (this.adaptedObject != null)
            {
                this.adaptedObject.RemoveInstance();
                this.adaptedObject.Dispose();
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        internal void SetAdaptedObject(PerformanceCounter adaptedObject)
        {
            this.adaptedObject = adaptedObject;
        }

        #endregion Methods
    }
}
