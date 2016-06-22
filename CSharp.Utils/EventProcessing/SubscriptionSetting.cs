using System;

namespace CSharp.Utils.EventProcessing
{
    [Serializable]
    public class SubscriptionSetting
    {
        public string Subject { get; set; }

        public bool NotifyInSameTransaction { get; set; }
    }
}
