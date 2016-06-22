using System;
using System.Web.UI;

namespace CSharp.Utils.Web
{
    public abstract class AbstractChildUserControl : UserControl
    {
        #region Public Properties

        public Control MasterControl { get; private set; }

        public abstract string MasterControlVirtualPath { get; }

        #endregion Public Properties

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            this.MasterControl = this.LoadControl(this.MasterControlVirtualPath);
            this.Controls.Add(this.MasterControl);

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.MasterControl.RenderControl(writer);
        }

        #endregion Methods
    }
}
