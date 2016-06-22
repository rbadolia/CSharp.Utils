using System;
using System.Threading;

namespace CSharp.Utils.Net
{
    public sealed class TelnetHelper : AbstractDisposable
    {
        #region Fields

        private readonly AutoResetEvent _newResultWaitHandle = new AutoResetEvent(false);

        private readonly TelnetWrapper tWrapper;

        private volatile string _expectedString;

        private volatile string _result;

        #endregion Fields

        #region Constructors and Finalizers

        internal TelnetHelper(TelnetWrapper wrapper, string commandPrompt)
        {
            this.tWrapper = wrapper;
            this.tWrapper.DataAvailable += this.tWrapper_DataAvailable;
        }

        ~TelnetHelper()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public static TelnetHelper CreateTelnetSession(string hostName, int port, string commandPrompt, string loginPrompt, string login, string passwordPrompt, string password, EventHandler<OnExceptionEventArgs> errorHandler, EventHandler disconnectedEventHandler)
        {
            var tWrapper = new TelnetWrapper();
            tWrapper.OnException += errorHandler;
            tWrapper.Disconnected += disconnectedEventHandler;
            tWrapper.HostName = hostName;
            tWrapper.Port = port;
            var helper = new TelnetHelper(tWrapper, commandPrompt);
            tWrapper.Connect();
            helper.WaitFor(loginPrompt);
            helper.SendCommand(login, passwordPrompt, 0);
            helper.SendCommand(password, commandPrompt, 0);
            return helper;
        }

        public void Disconnect()
        {
            if (this.tWrapper.Connected)
            {
                this.tWrapper.Disconnect();
            }
        }

        public string SendCommand(string command, string expectedString, int timeoutInMilliSeconds)
        {
            this._result = null;
            this._newResultWaitHandle.Reset();
            this._expectedString = expectedString;
            this.tWrapper.Send(command + this.tWrapper.CRLF);
            this.tWrapper.Receive();
            if (!string.IsNullOrEmpty(expectedString))
            {
                this._newResultWaitHandle.WaitOne(timeoutInMilliSeconds);
            }

            string s = this._result;
            this._result = null;
            this._expectedString = null;
            return s;
        }

        public void WaitFor(string expectedString)
        {
            this._result = null;
            this._newResultWaitHandle.Reset();
            this._expectedString = expectedString;
            this.tWrapper.Receive();
            if (!string.IsNullOrEmpty(expectedString))
            {
                this._newResultWaitHandle.WaitOne();
            }

            this._result = null;
            this._expectedString = null;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.Disconnect();
            this._newResultWaitHandle.Dispose();
        }

        private void tWrapper_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            this._result += e.Data;
            if (!string.IsNullOrEmpty(this._expectedString))
            {
                if (e.Data.Contains(this._expectedString) || (this._result != null && this._result.Contains(this._expectedString)))
                {
                    this._newResultWaitHandle.Set();
                }
            }
        }

        private void tWrapper_OnException(Exception e)
        {
        }

        #endregion Methods
    }
}
