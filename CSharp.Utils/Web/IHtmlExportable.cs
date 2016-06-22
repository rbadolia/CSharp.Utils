using System.IO;

namespace CSharp.Utils.Web
{
    public interface IHtmlExportable
    {
        #region Public Methods and Operators

        void ExportAsHtml(TextWriter writer);

        #endregion Public Methods and Operators
    }
}
