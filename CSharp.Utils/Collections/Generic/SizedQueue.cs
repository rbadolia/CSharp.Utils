using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSharp.Utils.Collections.Generic
{
    public class SizedQueue<T>
    {
        #region Fields

        private readonly Queue<T> adaptedObject = new Queue<T>();

        #endregion Fields

        #region Constructors and Finalizers

        public SizedQueue(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException(@"Queue size should be >0", "size");
            }

            this.Size = size;
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler<SizedQueueOnFullEventArgs<T>> OnQueueFull;

        #endregion Public Events

        #region Public Properties

        public int Count
        {
            get
            {
                return this.adaptedObject.Count;
            }
        }

        public int Size { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public T Dequeue()
        {
            return this.adaptedObject.Dequeue();
        }

        public void Enqueue(T item)
        {
            if (this.adaptedObject.Count == this.Size)
            {
                T oldItem = this.adaptedObject.Dequeue();
                if (this.OnQueueFull != null)
                {
                    try
                    {
                        this.OnQueueFull(this, new SizedQueueOnFullEventArgs<T>(oldItem));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            this.adaptedObject.Enqueue(item);
        }

        public void Enqueue(IEnumerable<T> enumerable)
        {
            foreach (T item in enumerable)
            {
                this.Enqueue(item);
            }
        }

        #endregion Public Methods and Operators
    }
}
