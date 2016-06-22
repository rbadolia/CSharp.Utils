using System;
using System.Net;

namespace CSharp.Utils.Net
{
    public class HttpListenerWrapper : AbstractHttpListenerWrapper
    {
        #region Public Events

        public event EventHandler<HttpRequestReceivedEventArgs> OnRequestReceived;

        #endregion Public Events

        #region Methods

        protected override void processRequest(HttpListenerContext context)
        {
            if (this.OnRequestReceived != null)
            {
                this.OnRequestReceived(this, new HttpRequestReceivedEventArgs(context));
            }
        }

        #endregion Methods
    }

    public class HttpRequestReceivedEventArgs : EventArgs
    {
        #region Constructors and Finalizers

        public HttpRequestReceivedEventArgs(HttpListenerContext context)
        {
            this.Context = context;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public HttpListenerContext Context { get; private set; }

        #endregion Public Properties
    }
}
