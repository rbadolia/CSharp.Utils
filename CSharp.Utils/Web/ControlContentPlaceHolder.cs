using System.Web.UI;

namespace CSharp.Utils.Web
{
    public class ControlContentPlaceHolder : Control
    {
        #region Methods

        protected override void Render(HtmlTextWriter writer)
        {
            ControlContent found = null;
            foreach (Control c in this.NamingContainer.NamingContainer.Controls)
            {
                var search = c as ControlContent;
                if (search != null && search.ControlContentPlaceHolderId.Equals(this.ID))
                {
                    found = search;
                    break;
                }
            }

            if (found != null)
            {
                found.RenderControl(writer);
            }
            else
            {
                base.Render(writer);
            }
        }

        #endregion Methods
    }
}
