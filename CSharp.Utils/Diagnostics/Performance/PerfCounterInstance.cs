using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSharp.Utils.Diagnostics.Performance
{
    [Serializable]
    public sealed class PerfCounterInstance : AbstractDisposable
    {
        #region Fields

        [NonSerialized]
        private GCHandle _referenceObject;

        #endregion Fields

        #region Constructors and Finalizers

        public PerfCounterInstance()
        {
            this.Counters = new List<PerfCounter>();
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public List<PerfCounter> Counters { get; private set; }

        public string InstanceName { get; set; }

        public object ReferenceObject
        {
            get
            {
                return this._referenceObject.Target;
            }

            set
            {
                if (this._referenceObject.IsAllocated)
                {
                    object o = this._referenceObject.Target;
                    if (o != null)
                    {
                        this._referenceObject.Free();
                    }
                }

                if (value != null)
                {
                    this._referenceObject = GCHandle.Alloc(value, GCHandleType.WeakTrackResurrection);
                }
            }
        }

        #endregion Public Properties

        #region Methods

        protected override void Dispose(bool disposing)
        {
            foreach (PerfCounter counter in this.Counters)
            {
                counter.Dispose();
            }

            this.Counters.Clear();
            if (this._referenceObject.IsAllocated)
            {
                object o = this._referenceObject.Target;
                if (o != null)
                {
                    this._referenceObject.Free();
                }
            }
        }

        #endregion Methods
    }
}
