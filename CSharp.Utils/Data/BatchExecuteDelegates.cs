using System.Data;

namespace CSharp.Utils.Data
{

    #region Delegates

    public delegate void AfterExecuteDelegate<in T>(IDbCommand command, T dataTransferObject, object returnedValue, object tag);

    public delegate void BeforeExecuteDelegate<in T>(IDbCommand command, T dataTransferObject, object tag);

    public delegate void InitializeCommandDelegate(IDbCommand command, object tag);

    #endregion Delegates
}
