using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using CSharp.Utils.Diagnostics.Performance.Dtos;

namespace CSharp.Utils.Diagnostics.Performance
{

    #region Delegates

    [GeneratedCode("wsdl", "4.0.30319.1")]
    public delegate void PublishCountersCompletedEventHandler(object sender, AsyncCompletedEventArgs e);

    #endregion Delegates

    [GeneratedCode("wsdl", "4.0.30319.1")]
    [DesignerCategory(@"code")]
    [WebServiceBinding(Name = "CounterSubscriberServiceSoap", Namespace = "http://tempuri.org/")]
    public sealed class CounterPublisherProxy : SoapHttpClientProtocol
    {
        #region Static Fields

        private static readonly CounterPublisherProxy InstanceObject = new CounterPublisherProxy();

        #endregion Static Fields

        #region Fields

        private SendOrPostCallback PublishCountersOperationCompleted;

        private bool enablePublishing;

        #endregion Fields

        #region Constructors and Finalizers

        private CounterPublisherProxy()
        {
            this.enablePublishing = false;
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event PublishCountersCompletedEventHandler PublishCountersCompleted;

        #endregion Public Events

        #region Public Properties

        public static CounterPublisherProxy Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public bool EnablePublishing
        {
            get
            {
                return this.enablePublishing;
            }

            set
            {
                if (this.enablePublishing != value)
                {
                    if (this.enablePublishing)
                    {
                        PerfCounterManager.Instance.AfterCountersCaptured -= this.Instance_AfterCountersCaptured;
                    }
                    else
                    {
                        PerfCounterManager.Instance.AfterCountersCaptured += this.Instance_AfterCountersCaptured;
                    }

                    this.enablePublishing = value;
                }
            }
        }

        public long Interval { get; set; }

        public DateTime? LastPublishedAt { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public IAsyncResult BeginPublishCounters(CounterUpdateInfo info, AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("PublishCounters", new object[] { info }, callback, asyncState);
        }

        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }

        public void EndPublishCounters(IAsyncResult asyncResult)
        {
            this.EndInvoke(asyncResult);
        }

        [SoapDocumentMethod("http://tempuri.org/PublishCounters", RequestNamespace = "http://tempuri.org/", ResponseNamespace = "http://tempuri.org/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public void PublishCounters(CounterUpdateInfo info)
        {
            try
            {
                this.Invoke("PublishCounters", new object[] { info });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void PublishCountersAsync(CounterUpdateInfo info)
        {
            this.PublishCountersAsync(info, null);
        }

        public void PublishCountersAsync(CounterUpdateInfo info, object userState)
        {
            if (this.PublishCountersOperationCompleted == null)
            {
                this.PublishCountersOperationCompleted = this.OnPublishCountersOperationCompleted;
            }

            this.InvokeAsync("PublishCounters", new object[] { info }, this.PublishCountersOperationCompleted, userState);
        }

        #endregion Public Methods and Operators

        #region Methods

        private void Instance_AfterCountersCaptured(object sender, CountersCapturedEventArgs e)
        {
            if (this.EnablePublishing)
            {
                if (this.LastPublishedAt == null || this.Interval < 0 || GlobalSettings.Instance.CurrentDateTime >= this.LastPublishedAt.Value.AddMilliseconds(this.Interval))
                {
                    this.LastPublishedAt = GlobalSettings.Instance.CurrentDateTime;
                    this.PublishCounters(e.Info);
                }
            }
        }

        private void OnPublishCountersOperationCompleted(object arg)
        {
            if (this.PublishCountersCompleted != null)
            {
                var invokeArgs = (InvokeCompletedEventArgs)arg;
                this.PublishCountersCompleted(this, new AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        #endregion Methods
    }
}
