using System.Net.Sockets;

namespace CSharp.Utils.Net
{
    public class TelnetState
    {
        #region Constants

        public const int BufferSize = 256;

        #endregion Constants

        #region Fields

        public byte[] Buffer = new byte[BufferSize];

        public Socket WorkSocket = null;

        #endregion Fields
    }
}
