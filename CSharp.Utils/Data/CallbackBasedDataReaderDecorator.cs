using System.Data;

namespace CSharp.Utils.Data
{

    #region Delegates

    public delegate bool CanReadCallback(IDataReader reader);

    #endregion Delegates

    public sealed class CallbackBasedDataReaderDecorator : DataReaderDecorator
    {
        #region Fields

        private readonly CanReadCallback _canReadCallback;

        #endregion Fields

        #region Constructors and Finalizers

        public CallbackBasedDataReaderDecorator(IDataReader adaptedObject, CanReadCallback canReadCallback)
            : base(adaptedObject)
        {
            this._canReadCallback = canReadCallback;
        }

        #endregion Constructors and Finalizers

        #region Methods

        protected override bool readProtected()
        {
            return this._canReadCallback(this);
        }

        #endregion Methods
    }
}
