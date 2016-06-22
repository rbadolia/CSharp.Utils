using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CSharp.Utils.Threading;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Collections.ObjectModel
{
    public class DispatcherAwareObservableCollection<T> : ObservableCollection<T>
    {
        private readonly IDispatcherProvider dispatcherProvider;

        private event NotifyCollectionChangedEventHandler collectionChanged;

        private event PropertyChangedEventHandler propertyChanged;

        protected override event PropertyChangedEventHandler PropertyChanged
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

        public DispatcherAwareObservableCollection(IDispatcherProvider dispatcherProvider)
        {
            Guard.ArgumentNotNull(dispatcherProvider, "dispatcherProvider");
            this.dispatcherProvider = dispatcherProvider;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var evt = this.collectionChanged;
            if (evt != null)
            {
                using (this.BlockReentrancy())
                {
                    GeneralHelper.SafeInvoke(evt, this, e);
                }
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var evt = this.propertyChanged;
            if (evt != null)
            {
                GeneralHelper.SafeInvoke(evt, this, e);
            }
        }
    }
}
