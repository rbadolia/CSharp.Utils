using System.Collections.Generic;
using System.Net.Mail;

namespace CSharp.Utils.Net.Mail
{
    public static class MailHelper
    {
        public static void PopulateMailAddressCollection(MailAddressCollection collection, IEnumerable<string> addresses)
        {
            foreach (var address in addresses)
            {
                var mailAddress = new MailAddress(address);
                collection.Add(mailAddress);
            }
        }
    }
}
