using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace CSharp.Utils.Web
{
    public static class WebHelper
    {
        #region Public Methods and Operators

        public static string GetControlValue(Control selectedControl)
        {
            if ((selectedControl is TextBox) || (selectedControl is HtmlInputText))
            {
                return (selectedControl is TextBox) ? ((TextBox)selectedControl).Text : ((HtmlInputText)selectedControl).Value;
            }

            var list = selectedControl as DropDownList;
            if (list != null)
            {
                return list.SelectedValue;
            }

            if ((selectedControl is CheckBox) || (selectedControl is HtmlInputCheckBox))
            {
                return (selectedControl is CheckBox) ? ((CheckBox)selectedControl).Checked.ToString(CultureInfo.InvariantCulture) : ((HtmlInputCheckBox)selectedControl).Checked.ToString(CultureInfo.InvariantCulture);
            }

            var label = selectedControl as Label;
            if (label != null)
            {
                return label.Text;
            }

            return string.Empty;
        }

        public static void PushFileToBrowser(HttpResponse response, byte[] bytes, string attachmentFileName)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                PushFileToBrowser(response, stream, bytes.Length, attachmentFileName);
            }
        }

        public static void PushFileToBrowser(HttpResponse response, Stream inputStream, long length, string attachmentFileName)
        {
            response.Clear();
            response.Buffer = false;
            response.AddHeader("Accept-Ranges", "bytes");
            response.ContentType = "application/octet-stream";
            response.AddHeader("Content-Disposition", "attachment;filename=" + attachmentFileName);
            response.AddHeader("Content-Length", length.ToString(CultureInfo.InvariantCulture));
            response.AddHeader("Connection", "Keep-Alive");
            var maxCount = (int)(length / 1024);
            var lastChunkSize = (int)(length % 1024);
            int icount;
            var buffer = new byte[1024];
            for (icount = 0; icount < maxCount && response.IsClientConnected; icount++)
            {
                int readCount = inputStream.Read(buffer, 0, buffer.Length);
                response.BinaryWrite(buffer);
                response.Flush();
            }

            if (lastChunkSize > 0)
            {
                var smallBuffer = new byte[lastChunkSize];
                inputStream.Read(smallBuffer, 0, smallBuffer.Length);
                response.BinaryWrite(smallBuffer);
                response.Flush();
            }

            response.End();
        }

        public static void PushFileToBrowser(HttpResponse response, string fileName)
        {
            var fi = new FileInfo(fileName);

            using (FileStream stream = File.OpenRead(fileName))
            {
                PushFileToBrowser(response, stream, fi.Length, fi.Name);
            }
        }

        #endregion Public Methods and Operators
    }
}
