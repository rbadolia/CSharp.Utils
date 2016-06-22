using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using CSharp.Utils.Threading;

namespace CSharp.Utils.Collections.Generic
{
    public class ListNotifyCollectionChangedAdapter<T> : AbstractListDecorator<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly IAtomicOperationSupported adaptedListAsAtomicOperationSupported;

        public ListNotifyCollectionChangedAdapter(IList<T> adaptedList)
            : base(adaptedList)
        {
            this.adaptedListAsAtomicOperationSupported = adaptedList as IAtomicOperationSupported;
        }

        public override void Add(T item)
        {
            var action = new Action(delegate
                {
                    base.Add(item);
                    int index = base.Count - 1;

                    this.OnCountChanged();
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                });
            this.PerformAction(action);
        }

        public override void Clear()
        {
            var action = new Action(delegate
                {
                    base.Clear();
                    this.OnCountChanged();
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
            this.PerformAction(action);
        }

        public override bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index > -1)
            {
                var action = new Action(delegate
                    {
                        bool returnValue = base.Remove(item);
                        if (returnValue)
                        {
                            this.OnCountChanged();
                            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                        }
                    });
                this.PerformAction(action);
                return true;
            }

            return false;
        }

        public override void Insert(int index, T item)
        {
            var action = new Action(delegate
                {
                    base.Insert(index, item);
                    this.OnCountChanged();
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                });
            this.PerformAction(action);
        }

        public override void RemoveAt(int index)
        {
            var action = new Action(delegate
                {
                    T item = base[index];
                    base.RemoveAt(index);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                    this.OnCountChanged();
                });
            this.PerformAction(action);
        }

        public override T this[int index]
        {
            set
            {
                var action = new Action(delegate
                    {
                        var oldValue = base[index];
                        base[index] = value;
                        this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldValue, value, index));
                    });
                this.PerformAction(action);
            }
        }

        #region Methods

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var collectionChanged = this.CollectionChanged;
            if (collectionChanged != null)
            {
                collectionChanged(this, e);
            }
        }

        private void OnCountChanged()
        {
            var countChangedEventArgs = new PropertyChangedEventArgs("Count");
            var itemArrayChangedEventArgs = new PropertyChangedEventArgs("Item[]");
            this.OnPropertyChanged(countChangedEventArgs);
            this.OnPropertyChanged(itemArrayChangedEventArgs);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, e);
            }
        }

        #endregion Methods

        private void PerformAction(Action action)
        {
            if (this.adaptedListAsAtomicOperationSupported == null)
            {
                action();
            }
            else
            {
                this.adaptedListAsAtomicOperationSupported.PerformAtomicOperation(action);
            }
        }
    }
}
