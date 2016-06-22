using System.Data;
using System.IO;
using System.Net;
using System.Text;
using CSharp.Utils.Data;
using CSharp.Utils.Net;
using CSharp.Utils.Web;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class PerfCounterHttpListener : AbstractHttpListenerWrapper
    {
        #region Static Fields

        private static readonly PerfCounterHttpListener InstanceObject = new PerfCounterHttpListener();

        #endregion Static Fields

        #region Constructors and Finalizers

        private PerfCounterHttpListener()
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static PerfCounterHttpListener Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Methods

        protected override void processRequest(HttpListenerContext context)
        {
            string categoryName = context.Request.QueryString["CategoryName"];

            using (var writer = new StringWriter())
            {
                HtmlExportHelper.WriteStyledHtmlHeader(writer, HtmlExportHelper.TableStyle, HtmlExportHelper.HeaderCellStyle, HtmlExportHelper.NumericCellStyle, HtmlExportHelper.CellStyle, "Performance Counters");
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    foreach (PerfCounterCategory category in
                        PerfCounterManager.Instance.GetAllCategories())
                    {
                        this.writeCategory(category, writer);
                    }
                }
                else
                {
                    PerfCounterCategory category = PerfCounterManager.Instance.GetCategory(categoryName);
                    if (category != null)
                    {
                        this.writeCategory(category, writer);
                    }
                }

                writer.Write("</body></html>");
                byte[] responseBytes = Encoding.ASCII.GetBytes(writer.ToString());
                context.Response.ContentLength64 = responseBytes.Length;
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                context.Response.ContentType = "text/html";
                context.Response.Close();
            }
        }

        private void writeCategory(PerfCounterCategory category, TextWriter writer)
        {
            DataTable table = category.CounterInstances.Count == 1 ? category.ExportAsTransposedDataTable(false) : category.ExportAsDataTable(false, false);
            var adapter = new DataReaderDataTableAdapter(table);
            HtmlExportHelper.WriteTableHeader(writer, adapter, true, category.CategoryName, "text-align:left");
            HtmlExportHelper.WriteRowsHtml(writer, adapter, true);
            writer.Write("</tbody></table><br/>");
        }

        #endregion Methods
    }
}
