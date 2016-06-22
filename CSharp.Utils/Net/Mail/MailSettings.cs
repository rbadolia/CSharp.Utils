using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace CSharp.Utils.Net.Mail
{
    [Serializable]
    public class MailSettings
    {
        public MailSettings()
        {
            this.From = MailClient.Instance.DefaultFromAddress;

            this.ToList = new List<string>();
            this.CCList = new List<string>();
            this.BCCList = new List<string>();
        }

        public string From { get; set; }

        public List<string> ToList { get; set; }

        public List<string> CCList { get; set; }

        public List<string> BCCList { get; set; }

        public string Subject { get; set; }

        public MailMessage CreateMailMessage()
        {
            var message = new MailMessage();
            message.From = new MailAddress(this.From);
            MailHelper.PopulateMailAddressCollection(message.To, this.ToList);
            MailHelper.PopulateMailAddressCollection(message.CC, this.CCList);
            MailHelper.PopulateMailAddressCollection(message.Bcc, this.BCCList);
            return message;
        }
    }
}
