using System;
using System.Runtime.Serialization;

namespace CSharp.Utils.RequestProcessing
{
    [Serializable]
    [DataContract]
    public abstract class AbstractRequest : IRequest
    {
        #region Fields

        private string requestId = Guid.NewGuid().ToString();

        #endregion Fields

        #region Public Properties

        [DataMember]
        public virtual string RequestId
        {
            get
            {
                return this.requestId;
            }

            set
            {
                this.requestId = value;
            }
        }

        #endregion Public Properties
    }
}
