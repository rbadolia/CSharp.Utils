using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Data
{
    [Serializable]
    public class DbCommandExecuteException : ApplicationException
    {
        #region Constructors and Finalizers

        public DbCommandExecuteException(string message, Exception innerException, object dataTransferObject)
            : base(message, innerException)
        {
            this.DataTransferObject = dataTransferObject;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DbCommandExecuteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            string dtoTypeName = info.GetString("DtoTypeName");
            if (!string.IsNullOrEmpty(dtoTypeName))
            {
                try
                {
                    Type dtoType = Type.GetType(dtoTypeName);
                    if (dtoType != null)
                    {
                        this.DataTransferObject = info.GetValue("DataTransferObject", dtoType);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public object DataTransferObject { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.ArgumentNotNull(info, "info");
            try
            {
                info.AddValue("DtoTypeName", this.DataTransferObject == null ? string.Empty : this.DataTransferObject.GetType().FullName);
                if (this.DataTransferObject != null)
                {
                    info.AddValue("DataTransferObject", this.DataTransferObject, this.DataTransferObject.GetType());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            base.GetObjectData(info, context);
        }

        #endregion Public Methods and Operators
    }
}
