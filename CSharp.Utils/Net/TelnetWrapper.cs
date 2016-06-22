using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CSharp.Utils.Net
{
    public sealed class TelnetWrapper : TelnetProtocolHandler, IDisposable
    {
        #region Fields

        private readonly ManualResetEvent _connectedWaitHandle = new ManualResetEvent(false);

        private readonly ManualResetEvent _sentWaitHandle = new ManualResetEvent(false);

        private bool _hasExceptionOccured;

        private int _port;

        private Socket _socket;

        #endregion Fields

        #region Constructors and Finalizers

        ~TelnetWrapper()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler<DataAvailableEventArgs> DataAvailable;

        public event EventHandler Disconnected;

        public event EventHandler<OnExceptionEventArgs> OnException;

        #endregion Public Events

        #region Public Properties

        public bool Connected
        {
            get
            {
                return this._socket.Connected;
            }
        }

        public string HostName { get; set; }

        public int Port
        {
            get
            {
                return this._port;
            }

            set
            {
                if (value > 0)
                {
                    this._port = value;
                }
                else
                {
                    throw new ArgumentException(@"Port number must be greater than 0.", "value");
                }
            }
        }

        public string TerminalType { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public void Close()
        {
            this.Dispose();
        }

        public void Connect()
        {
            this.Connect(this.HostName, this.Port);
        }

        public void Connect(string host, int port)
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.Resolve(host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                var remoteEndPoint = new IPEndPoint(ipAddress, port);
                this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._socket.BeginConnect(remoteEndPoint, this.ConnectCallback, this._socket);
                this._connectedWaitHandle.WaitOne();

                this.Reset();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.Disconnect();
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (this._socket != null && this._socket.Connected)
                {
                    this._socket.Shutdown(SocketShutdown.Both);
                    this._socket.Close();
                    this.Disconnected(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Receive()
        {
            this.Receive(this._socket);
        }

        public string Send(string cmd)
        {
            try
            {
                byte[] arr = Encoding.ASCII.GetBytes(cmd);
                this.Transpose(arr);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.Disconnect();
                throw new ApplicationException("Error writing to _socket.", ex);
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void NotifyEndOfRecord()
        {
        }

        protected override void SetLocalEcho(bool echo)
        {
        }

        protected override void Write(byte[] b)
        {
            if (this._socket.Connected)
            {
                this.Send(this._socket, b);
            }

            this._sentWaitHandle.WaitOne();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var client = (Socket)ar.AsyncState;

            try
            {
                client.EndConnect(ar);
                this._connectedWaitHandle.Set();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.Disconnect();
                if (this.OnException != null)
                {
                    this.OnException(this, new OnExceptionEventArgs(ex));
                }
                else
                {
                    throw new ApplicationException("Unable to connect to " + client.RemoteEndPoint, ex);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            this._connectedWaitHandle.Dispose();
            this._sentWaitHandle.Dispose();
            if (disposing)
            {
                this.Disconnect();
            }

            this._socket.Dispose();
        }

        private void Receive(Socket client)
        {
            try
            {
                var state = new TelnetState { WorkSocket = client };
                var error = SocketError.Success;
                client.BeginReceive(state.Buffer, 0, TelnetState.BufferSize, 0, out error, this.ReceiveCallback, state);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.Disconnect();
                if (this.OnException != null)
                {
                    this.OnException(this, new OnExceptionEventArgs(ex));
                }
                else
                {
                    throw new ApplicationException("Error on read from socket.", ex);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (TelnetState)ar.AsyncState;
                Socket client = state.WorkSocket;
                if (!this._hasExceptionOccured)
                {
                    int bytesRead = client.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        this.InputFeed(state.Buffer, bytesRead);
                        this.Negotiate(state.Buffer);

                        this.DataAvailable(this, new DataAvailableEventArgs(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead)));
                        client.BeginReceive(state.Buffer, 0, TelnetState.BufferSize, 0, this.ReceiveCallback, state);
                    }
                    else
                    {
                        this.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                if (!this._hasExceptionOccured)
                {
                    this._hasExceptionOccured = true;
                    this.Disconnect();

                    if (this.OnException != null)
                    {
                        this.OnException(this, new OnExceptionEventArgs(ex));
                    }
                    else
                    {
                        throw new Exception("Error reading from socket.", ex);
                    }
                }
            }
        }

        private void Send(Socket client, byte[] byteData)
        {
            client.BeginSend(byteData, 0, byteData.Length, 0, this.SendCallback, client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            var client = (Socket)ar.AsyncState;
            int bytesSent = client.EndSend(ar);
            this._sentWaitHandle.Set();
        }

        #endregion Methods
    }
}
