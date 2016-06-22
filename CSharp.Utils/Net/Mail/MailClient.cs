using System.Net;
using System.Net.Mail;

namespace CSharp.Utils.Net.Mail
{
    public sealed class MailClient
    {
        private static MailClient _instance = new MailClient();

        private MailClient()
        {

        }

        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public string DefaultFromAddress { get; set; }

        public static MailClient Instance { get { return _instance; } }

        public void SendMail(MailMessage message)
        {
            var client = new SmtpClient(this.Host);
            client.Port = this.Port;
            client.Credentials=new NetworkCredential(this.UserName, this.Password, this.Domain);
            client.UseDefaultCredentials = false;
            client.Send(message);
        }
    }
}
