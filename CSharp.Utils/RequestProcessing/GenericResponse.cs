using System;
using System.Runtime.Serialization;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.RequestProcessing
{
    [Serializable]
    [DataContract]
    public class GenericResponse : IResponse
    {
        #region Constructors and Finalizers

        public GenericResponse()
        {
            this.Result = ProcessingResult.Fail;
        }

        #endregion

        #region Public Properties

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public Exception Exception { get; set; }

        [DataMember(IsRequired = true)]
        public virtual string RequestId { get; set; }

        [DataMember(IsRequired = true)]
        public ProcessingResult Result { get; set; }

        public override string ToString()
        {
        	return DynamicToStringHelper<GenericResponse>.ExportAsString(this);
        }

        #endregion
    }
}
