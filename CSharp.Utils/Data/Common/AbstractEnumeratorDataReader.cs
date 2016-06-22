using System.Collections.Generic;

namespace CSharp.Utils.Data.Common
{
    public abstract class AbstractEnumeratorDataReader<T> : AbstractDataReader
    {
        #region Fields

        private IEnumerator<T> enumerator;

        #endregion Fields

        #region Constructors and Finalizers

        protected AbstractEnumeratorDataReader()
        {
            this.SequenceNumber = 0;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public long SequenceNumber { get; private set; }

        #endregion Public Properties

        #region Properties

        protected T current { get; private set; }

        #endregion Properties

        #region Public Methods and Operators

        public void SetEnumerator(IEnumerator<T> enumerator, bool resetSequenceNumber)
        {
            this.enumerator = enumerator;
            this.current = default(T);
            if (resetSequenceNumber)
            {
                this.SequenceNumber = 0;
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override bool readProtected()
        {
            if (this.enumerator.MoveNext())
            {
                this.SequenceNumber++;
                this.current = this.enumerator.Current;
                return true;
            }

            this.enumerator.Dispose();
            return false;
        }

        #endregion Methods
    }
}
