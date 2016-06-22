using System.Threading;

namespace CSharp.Utils.IO
{
    public interface IFileQueue
    {
        #region Public Properties

        WaitHandle NewFileWaitHandle { get; }

        #endregion Public Properties

        #region Public Methods and Operators

        string GetNextFile();

        #endregion Public Methods and Operators
    }
}
