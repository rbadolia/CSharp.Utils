using System.Data;
using System.Web;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class PerfCounterHttpHandler : IHttpHandler
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
            string categoryName = context.Request.QueryString["CategoryName"];

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                string includeGroupNames = context.Request.QueryString["IncludeGroupNames"];
                bool shouldIncludeGroupNames = false;
                if (string.IsNullOrWhiteSpace(includeGroupNames))
                {
                    bool.TryParse(includeGroupNames, out shouldIncludeGroupNames);
                }

                PerfCounterCategory category = PerfCounterManager.Instance.GetCategory(categoryName);
                if (category != null)
                {
                    DataTable table = category.ExportAsTransposedDataTable(shouldIncludeGroupNames);
                    context.Response.ContentType = "application/xml";
                    table.WriteXml(context.Response.OutputStream);
                    context.Response.OutputStream.Flush();
                }
            }
        }

        #endregion Public Methods and Operators
    }
}
