namespace CSharp.Utils.Net
{
    public sealed class PingTelnetHelper : AbstractDisposable
    {
        #region Fields

        private readonly string _commandPrompt;

        private readonly TelnetHelper _telnetHelper;

        #endregion Fields

        #region Constructors and Finalizers

        public PingTelnetHelper(TelnetHelper telnetHelper, string commandPrompt)
        {
            this._telnetHelper = telnetHelper;
            this._commandPrompt = commandPrompt;
        }

        ~PingTelnetHelper()
        {
            this.Dispose(false);
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public TelnetHelper TelnetHelper
        {
            get
            {
                return this._telnetHelper;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public decimal PingIpAddress(string ipAddress, IPingResultProcessor resultProcessor, int timeoutInMilliSeconds)
        {
            string r = this._telnetHelper.SendCommand("ping " + ipAddress, this._commandPrompt, timeoutInMilliSeconds);
            return resultProcessor.ProcessPingResult(r, this._commandPrompt);
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this._telnetHelper.Dispose();
        }

        #endregion Methods
    }
}
