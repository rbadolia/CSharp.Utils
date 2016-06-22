using System;
using System.Collections.Generic;
using System.Linq;
using CSharp.Utils.Diagnostics;
using CSharp.Utils.RequestProcessing;

namespace CSharp.Utils.EventProcessing.Abstractions
{
    public abstract class AbstractControllableEventSubscriber : AbstractControllableComponent
    {
        private List<IDisposable> _unsubscribeDisposables;

        private RequestHandlerMetrics _requestHandlerMetrics  = new RequestHandlerMetrics();

        public List<SubscriptionSetting> Subscriptions { get; set; }

        private void OnEventReceived(object sender, EventArg e)
        {
            this._requestHandlerMetrics.RequestReceived();
            var ticksBefore = SharedStopWatch.ElapsedTicks;
            if (!this.IsRunning)
            {
                this._requestHandlerMetrics.RequestIgnored();
                return;
            }

            var controller = (GenericMetricsEnabledComponentController) this.Controller;
            Exception exception = null;
            try
            {
                controller.StartedDoingSomething();
                this.OnEventReceivedProtected(e);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                controller.StoppedDoingSomething();
                this._requestHandlerMetrics.RequestProcessed(SharedStopWatch.ElapsedTicks - ticksBefore, 
                    exception != null);
            }
        }

        protected override void InitializeProtected()
        {
            base.InitializeProtected();
            this._unsubscribeDisposables = new List<IDisposable>();

            var groups = this.Subscriptions.GroupBy(x => x.NotifyInSameTransaction);
            foreach (var group in groups)
            {
                var disposable = EventPublisher.Instance.Subscribe(this.OnEventReceived, group.Key, 
                    group.Select(x => x.Subject).ToArray());
                this._unsubscribeDisposables.Add(disposable);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            GeneralHelper.DisposeIDisposables(this._unsubscribeDisposables);
        }

        protected abstract void OnEventReceivedProtected(EventArg e);

        public RequestHandlerMetrics GetMetrics()
        {
            return this._requestHandlerMetrics;
        }
    }
}
