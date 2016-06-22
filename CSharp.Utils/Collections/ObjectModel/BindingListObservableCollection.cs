using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Collections.ObjectModel
{
    public class BindingListObservableCollection<T> : ObservableCollection<T>, IBindingList
        where T : new()
    {
        #region Fields

        private readonly object syncLock = new object();

        private bool isSorted;

        private ListSortDirection sortDirection;

        private PropertyDescriptor sortProperty;

        #endregion Fields

        #region Constructors and Finalizers

        public BindingListObservableCollection()
        {
        }

        public BindingListObservableCollection(IEnumerable<T> elements)
        {
            foreach (T element in elements)
            {
                this.Add(element);
            }
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event ListChangedEventHandler ListChanged;

        #endregion Public Events

        #region Public Properties

        public bool AllowEdit
        {
            get
            {
                return true;
            }
        }

        public bool AllowNew
        {
            get
            {
                return true;
            }
        }

        public bool AllowRemove
        {
            get
            {
                return true;
            }
        }

        public bool IsSorted
        {
            get
            {
                return this.isSorted;
            }
        }

        public ListSortDirection SortDirection
        {
            get
            {
                return this.sortDirection;
            }
        }

        public PropertyDescriptor SortProperty
        {
            get
            {
                return this.sortProperty;
            }
        }

        public bool SupportsChangeNotification
        {
            get
            {
                return true;
            }
        }

        public bool SupportsSearching
        {
            get
            {
                return true;
            }
        }

        public bool SupportsSorting
        {
            get
            {
                return true;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void AddIndex(PropertyDescriptor property)
        {
        }

        public object AddNew()
        {
            int index;
            var t = new T();
            lock (this.syncLock)
            {
                this.Add(t);
                index = this.Count - 1;
            }

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, t));
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            return t;
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            var items = this.Items as List<T>;
            if (items != null)
            {
                var comparer = DynamicComparerHelper<T>.GetComparerForProperty(property.Name);
                if (comparer == null)
                {
                    return;
                }

                if (direction == ListSortDirection.Descending)
                {
                    comparer = new InverseComparer<T>(comparer);
                }

                items.Sort(comparer);
                this.isSorted = true;
            }
            else
            {
                this.isSorted = false;
            }

            this.sortProperty = property;
            this.sortDirection = direction;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public int Find(PropertyDescriptor property, object key)
        {
            if (property == null)
            {
                return -1;
            }

            IList<T> items = this.Items;
            foreach (T item in items)
            {
                if (item != null)
                {
                    var value = (string)property.GetValue(item);
                    if ((string)key == value)
                    {
                        return this.IndexOf(item);
                    }
                }
            }

            return -1;
        }

        public void RemoveIndex(PropertyDescriptor property)
        {
        }

        public void RemoveSort()
        {
            this.isSorted = false;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        #endregion Public Methods and Operators

        #region Methods

        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            if (this.ListChanged != null)
            {
                this.ListChanged(this, e);
            }
        }

        #endregion Methods
    }
}
