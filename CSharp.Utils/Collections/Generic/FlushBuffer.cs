using System;
using System.Collections.Generic;
using System.Threading;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Collections.Generic
{
    public sealed class FlushBuffer<T> : AbstractDisposable
    {
        #region Fields

        private readonly bool _keepOnlyUniqueItems;

        private readonly IntervalTask _scheduledTask;

        private readonly List<T> collection1 = new List<T>();

        private readonly List<T> collection2 = new List<T>();

        private int? _bufferSize = 1;

        private List<T> writingCollection;

        #endregion Fields

        #region Constructors and Finalizers

        public FlushBuffer(int? bufferSize, long? flushInterval, bool keepOnlyUniqueItems)
        {
            this.writingCollection = this.collection1;
            this._bufferSize = bufferSize;
            this._keepOnlyUniqueItems = keepOnlyUniqueItems;
            if (flushInterval != null)
            {
                this._scheduledTask = new IntervalTask(flushInterval.Value, false);
                this._scheduledTask.AddAction(this.Flush, null);
                this._scheduledTask.Start();
            }
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler<FlushEventArgs<T>> BeforeFlush;

        #endregion Public Events

        #region Public Methods and Operators

        public void Write(T obj)
        {
            List<T> coll = this.writingCollection;
            if (!this._keepOnlyUniqueItems || (this._keepOnlyUniqueItems && !coll.Contains(obj)))
            {
                coll.Add(obj);
                if (this._bufferSize != null && coll.Count >= this._bufferSize.Value)
                {
                    ThreadPool.QueueUserWorkItem(state => this.Flush(null), null);
                }
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this._scheduledTask != null)
            {
                this._scheduledTask.Dispose();
            }

            this.Flush(null);
        }

        private void Flush(object tag)
        {
            if (this.writingCollection.Count > 0)
            {
                List<T> coll = this.writingCollection;
                this.writingCollection = this.writingCollection == this.collection1 ? this.collection2 : this.collection1;
                if (this.BeforeFlush != null)
                {
                    this.BeforeFlush(this, new FlushEventArgs<T>(coll));
                }

                coll.Clear();
            }
        }

        #endregion Methods
    }
}
