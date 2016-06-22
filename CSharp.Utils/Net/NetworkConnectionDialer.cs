using System;
using System.Runtime.InteropServices;

namespace CSharp.Utils.Net
{

    #region Enumerations

    public enum ConnectionType
    {
        Lan = 0, 

        Modem = 1, 

        Proxy = 2, 

        NotConnected = 3
    }

    public enum DisconnectResult
    {
        Disconnected = 0, 

        CouldNotDisconnect = 1, 

        NoActiveConnectionToDisconnect = 2
    }

    #endregion Enumerations

    public class NetworkConnectionDialer
    {
        #region Constants

        private const int ERROR_INVALID_PARAMETER = 0X87;

        private const int ERROR_SUCCESS = 0X0;

        #endregion Constants

        #region Fields

        private readonly string _connectionName;

        private int mlConnection;

        #endregion Fields

        #region Constructors and Finalizers

        public NetworkConnectionDialer(string connectionName)
        {
            this._connectionName = connectionName;
        }

        #endregion Constructors and Finalizers
        #region Enums

        private enum DialUpOptions
        {
            INTERNET_DIAL_UNATTENDED = 0X8000, 

            INTERNET_DIAL_SHOW_OFFLINE = 0X4000, 

            INTERNET_DIAL_FORCE_PROMPT = 0X2000
        }

        private enum Flags
        {
            INTERNET_CONNECTION_LAN = 0X2, 

            INTERNET_CONNECTION_MODEM = 0X1, 

            INTERNET_CONNECTION_PROXY = 0X4, 

            INTERNET_RAS_INSTALLED = 0X10
        }

        #endregion Enums
        #region Public Properties

        public static bool IsConnected
        {
            get
            {
                int Desc = 0;
                return NativeMethods.InternetGetConnectedState(ref Desc, 0);
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public bool Connect()
        {
            int DResult = NativeMethods.InternetDial(IntPtr.Zero, this._connectionName, Convert.ToInt32(DialUpOptions.INTERNET_DIAL_UNATTENDED), ref this.mlConnection, 0);
            return DResult == ERROR_SUCCESS;
        }

        public bool Connect(int maxNumberOfAttempts)
        {
            if (maxNumberOfAttempts <= 0)
            {
                throw new ArgumentException(@"maxNumberOfAttempts must be greater than zero", "maxNumberOfAttempts");
            }

            for (int i = 0; i < maxNumberOfAttempts || maxNumberOfAttempts < 1; )
            {
                if (this.Connect())
                {
                    return true;
                }

                if (maxNumberOfAttempts > 0)
                {
                    i++;
                }
            }

            return false;
        }

        public DisconnectResult Disconnect()
        {
            if (this.mlConnection != 0)
            {
                int result = NativeMethods.InternetHangUp(this.mlConnection, 0);
                return result == 0 ? DisconnectResult.CouldNotDisconnect : DisconnectResult.Disconnected;
            }

            return DisconnectResult.NoActiveConnectionToDisconnect;
        }

        public ConnectionType GetConnectionType()
        {
            int lngFlags = 0;

            if (NativeMethods.InternetGetConnectedState(ref lngFlags, 0))
            {
                if ((lngFlags & (int)Flags.INTERNET_CONNECTION_LAN) != 0)
                {
                    return ConnectionType.Lan;
                }

                if ((lngFlags & (int)Flags.INTERNET_CONNECTION_MODEM) != 0)
                {
                    return ConnectionType.Modem;
                }

                if ((lngFlags & (int)Flags.INTERNET_CONNECTION_PROXY) != 0)
                {
                    return ConnectionType.Proxy;
                }

                return ConnectionType.NotConnected;
            }

            return ConnectionType.NotConnected;
        }

        #endregion Public Methods and Operators

        private static class NativeMethods
        {
            #region Methods

            [DllImport("Wininet.dll", EntryPoint = "InternetDial", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int InternetDial(IntPtr hwndParent, string lpszConnectoid, int dwFlags, ref int lpdwConnection, int dwReserved);

            [DllImport("wininet.dll", EntryPoint = "InternetGetConnectedState", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool InternetGetConnectedState(ref int lpdwFlags, int dwReserved);

            [DllImport("Wininet.dll", EntryPoint = "InternetHangUp", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int InternetHangUp(int lpdwConnection, int dwReserved);

            #endregion Methods
        }
    }
}
