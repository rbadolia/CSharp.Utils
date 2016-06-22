using System;
using System.Diagnostics;
using System.Net;
using CSharp.Utils.Contracts;

namespace CSharp.Utils.Diagnostics
{
    public class UrlHitStrategy : IConditionStrategy
    {
        #region Public Properties

        public int Timeout { get; set; }

        public string Url { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public bool ExecuteCondition()
        {
            WebRequest req = WebRequest.Create(this.Url);
            req.Timeout = this.Timeout;
            try
            {
                req.GetResponse();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        #endregion Public Methods and Operators
    }
}
