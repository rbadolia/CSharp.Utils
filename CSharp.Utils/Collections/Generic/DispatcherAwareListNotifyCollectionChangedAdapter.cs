using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using CSharp.Utils.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.Generic
{
    public class DispatcherAwareListNotifyCollectionChangedAdapter<T> : ListNotifyCollectionChangedAdapter<T>
    {
        private readonly IDispatcherProvider dispatcherProvider;

        private event PropertyChangedEventHandler propertyChanged;

        private event NotifyCollectionChangedEventHandler collectionChanged;

        public override event PropertyChangedEventHandler PropertyChanged
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
                    this.propertyChanged += value;
                }
                else
                {
                    this.propertyChanged += (sender, e) => dispatcher.Dispatch(() => value(sender, e));
                }
            }

            remove
            {
                this.propertyChanged -= value;
            }
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged
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
                    this.collectionChanged += value;
                }
                else
                {
                    this.collectionChanged += (sender, e) => dispatcher.Dispatch(() => value(sender, e));
                }
            }

            remove
            {
                this.collectionChanged -= value;
            }
        }

        public DispatcherAwareListNotifyCollectionChangedAdapter(IList<T> adaptedList, IDispatcherProvider dispatcherProvider)
            : base(adaptedList)
        {
            Guard.ArgumentNotNull(dispatcherProvider, "dispatcherProvider");
            this.dispatcherProvider = dispatcherProvider;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            GeneralHelper.SafeInvoke(this.collectionChanged, this, e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            GeneralHelper.SafeInvoke(this.propertyChanged, this, e);
        }
    }
}
