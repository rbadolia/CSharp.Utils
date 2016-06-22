using System.Web;

namespace CSharp.Utils.Web
{
    public class ReverseProxyHttpHandler : ReverseProxyHttpModule, IHttpHandler
    {
        #region Public Properties

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public void ProcessRequest(HttpContext context)
        {
            this.processRequest(context);
        }

        #endregion Public Methods and Operators
    }
}
