using CSharp.Utils.EventProcessing.Abstractions;
using CSharp.Utils.Net.Mail;

namespace CSharp.Utils.EventProcessing
{
    public class MailSendingEventSubscriber : AbstractControllableEventSubscriber
    {
        public MailSettings MailSettings { get; set; }

        protected override void OnEventReceivedProtected(EventArg e)
        {
            var message = this.MailSettings.CreateMailMessage();
            MailClient.Instance.SendMail(message);
        }
    }
}
