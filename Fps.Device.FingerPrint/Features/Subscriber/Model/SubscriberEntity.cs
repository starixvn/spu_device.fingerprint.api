using System;

namespace Fps.Device.FingerPrint.Features.Subscriber.Model
{
    public class SubscriberEntity
    {
        public int SubscriberId { get; set; }

        public string SubscriberName { get; set; }

        public bool SubscriberType { get; set; }

        public bool DCSStatus { get; set; }

        public bool SubscriberStatus { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
