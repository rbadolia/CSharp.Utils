using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Collections.Generic
{
    public class SearchableSortableBindingList<T> : BindingList<T>
    {
        #region Fields

        private T _current;

        private bool _isSorted;

        private ListSortDirection _sortDirection;

        private PropertyDescriptor _sortProperty;

        #endregion Fields

        #region Public Properties

        public T Current
        {
            get
            {
                return this._current;
            }

            set
            {
                if (this.Contains(value))
                {
                    this._current = value;
                }
                else
                {
                    throw new InvalidDataException("This item does n't exist in the collection");
                }
            }
        }

        public ListSortDirection SortDirection
        {
            get
            {
                return this._sortDirection;
            }
        }

        public PropertyDescriptor SortProperty
        {
            get
            {
                return this._sortProperty;
            }
        }

        #endregion Public Properties

        #region Properties

        protected override bool IsSortedCore
        {
            get
            {
                return this._isSorted;
            }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return this._sortDirection;
            }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return this._sortProperty;
            }
        }

        protected override bool SupportsSearchingCore
        {
            get
            {
                return true;
            }
        }

        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        #endregion Properties

        #region Public Methods and Operators

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            this.ApplySortCore(property, direction);
        }

        public void Load(string filename)
        {
            this.ClearItems();

            if (File.Exists(filename))
            {
                var formatter = new BinaryFormatter();
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    ((List<T>)this.Items).AddRange((IEnumerable<T>)formatter.Deserialize(stream));
                }
            }

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public void Load(IList<T> items)
        {
            foreach (T item in items)
            {
                this.Add(item);
            }
        }

        public void Save(string filename)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                formatter.Serialize(stream, this.Items);
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
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
                this._isSorted = true;
            }
            else
            {
                this._isSorted = false;
            }

            this._sortProperty = property;
            this._sortDirection = direction;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override int FindCore(PropertyDescriptor property, object key)
        {
            if (property == null)
            {
                return -1;
            }

            IList<T> items = this.Items;
            foreach (T item in items)
            {
                var value = (string)property.GetValue(item);
                if ((string)key == value)
                {
                    return this.IndexOf(item);
                }
            }

            return -1;
        }

        protected override void RemoveSortCore()
        {
            this._isSorted = false;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        #endregion Methods
    }
}
