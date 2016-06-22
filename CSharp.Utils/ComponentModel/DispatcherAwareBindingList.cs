using System;
using System.Collections.Generic;
using System.ComponentModel;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.ComponentModel
{
    public class DispatcherAwareBindingList<T> : AbstractListDecorator<T>, IBindingList,  ICancelAddNew, IRaiseItemChangedEvents
    {
        private IDispatcherProvider dispatcherProvider;

        private IBindingList bindingList;

        private ListChangedEventHandler listChangedEventHandler;

        private AddingNewEventHandler addingNewEventHandler;

        public DispatcherAwareBindingList(IDispatcherProvider dispatcherProvider)
            : base(new BindingList<T>())
        {
            this.Initialize(dispatcherProvider, null);
        }

        public DispatcherAwareBindingList(IDispatcherProvider dispatcherProvider, IList<T> adaptedList)
            : base(new BindingList<T>(adaptedList))
        {
            Guard.ArgumentNotNull(adaptedList, "adaptedList");
            this.Initialize(dispatcherProvider, adaptedList);
        }

        private void Initialize(IDispatcherProvider dispatcherProvider, IList<T> adaptedList)
        {
            Guard.ArgumentNotNull(dispatcherProvider, "dispatcherProvider");
            this.dispatcherProvider = dispatcherProvider;

            var bindingListObject = (BindingList<T>)this.AdaptedList;
            bindingListObject.AddingNew += bindingListObject_AddingNew;
            bindingListObject.ListChanged += bindingListObject_ListChanged;
            this.bindingList = bindingListObject;
        }

        private void bindingListObject_ListChanged(object sender, ListChangedEventArgs e)
        {
            GeneralHelper.SafeInvoke(this.listChangedEventHandler, this, e);
        }

        private void bindingListObject_AddingNew(object sender, AddingNewEventArgs e)
        {
            GeneralHelper.SafeInvoke(this.addingNewEventHandler, this, e);
        }

        public event ListChangedEventHandler ListChanged
        {
            add
            {
                IDispatcher dispatcher = null;
                if (this.dispatcherProvider != null)
                {
                    dispatcher = this.dispatcherProvider.GetDispatcher(value.Target);
                }

                if (dispatcher == null)
                {
                    this.listChangedEventHandler += value;
                }
                else
                {
                    this.listChangedEventHandler += (sender, e) => dispatcher.Dispatch(() => value(sender, e));
                }
            }

            remove
            {
                this.listChangedEventHandler -= value;
            }
        }

        public event AddingNewEventHandler AddingNew
        {
            add
            {
                IDispatcher dispatcher = null;
                if (this.dispatcherProvider != null)
                {
                    dispatcher = this.dispatcherProvider.GetDispatcher(value.Target);
                }

                if (dispatcher == null)
                {
                    this.addingNewEventHandler += value;
                }
                else
                {
                    this.addingNewEventHandler += (sender, e) => dispatcher.Dispatch(() => value(sender, e));
                }
            }

            remove
            {
                this.addingNewEventHandler -= value;
            }
        }

        public void AddIndex(PropertyDescriptor property)
        {
            this.bindingList.AddIndex(property);
        }

        public object AddNew()
        {
            return this.bindingList.AddNew();
        }

        public bool AllowEdit
        {
            get
            {
                return this.bindingList.AllowEdit;
            }
        }

        public bool AllowNew
        {
            get
            {
                return this.bindingList.AllowNew;
            }
        }

        public bool AllowRemove
        {
            get
            {
                return this.bindingList.AllowRemove;
            }
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            this.bindingList.ApplySort(property, direction);
        }

        public int Find(PropertyDescriptor property, object key)
        {
            return this.bindingList.Find(property, key);
        }

        public bool IsSorted
        {
            get
            {
                return this.bindingList.IsSorted;
            }
        }

        public void RemoveIndex(PropertyDescriptor property)
        {
            this.bindingList.RemoveIndex(property);
        }

        public void RemoveSort()
        {
            this.bindingList.RemoveSort();
        }

        public ListSortDirection SortDirection
        {
            get
            {
                return this.bindingList.SortDirection;
            }
        }

        public PropertyDescriptor SortProperty
        {
            get
            {
                return this.bindingList.SortProperty;
            }
        }

        public bool SupportsChangeNotification
        {
            get
            {
                return this.bindingList.SupportsChangeNotification;
            }
        }

        public bool SupportsSearching
        {
            get
            {
                return this.bindingList.SupportsSearching;
            }
        }

        public bool SupportsSorting
        {
            get
            {
                return this.bindingList.SupportsSorting;
            }
        }

        public int Add(object value)
        {
            return this.bindingList.Add(value);
        }

        public bool Contains(object value)
        {
            return this.bindingList.Contains(value);
        }

        public int IndexOf(object value)
        {
            return this.bindingList.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            this.bindingList.Insert(index, value);
        }

        public bool IsFixedSize
        {
            get
            {
                return this.bindingList.IsFixedSize;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.bindingList.IsReadOnly;
            }
        }

        public void Remove(object value)
        {
            this.bindingList.Remove(value);
        }

        public void CopyTo(Array array, int index)
        {
            this.bindingList.CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get
            {
                return this.bindingList.IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.bindingList.SyncRoot;
            }
        }

        public void CancelNew(int itemIndex)
        {
            ((ICancelAddNew)this.bindingList).CancelNew(itemIndex);
        }

        public void EndNew(int itemIndex)
        {
            ((ICancelAddNew)this.bindingList).EndNew(itemIndex);
        }

        public bool RaisesItemChangedEvents
        {
            get
            {
                return ((IRaiseItemChangedEvents)this.bindingList).RaisesItemChangedEvents;
            }
        }
    }
}
