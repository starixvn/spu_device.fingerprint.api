using System;
using System.Collections.Generic;
using System.Text;

namespace FPS.Device.FingerPrint.Net.Framework.Client.Model
{
    public class SubscriberModel
    {
        public int SubscriberId { get; set; }

        public string SubscriberName { get; set; }

        public bool SubscriberType { get; set; }

        public bool DCSStatus { get; set; }

        public bool SubscriberStatus { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
