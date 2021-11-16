using System;
using System.Collections.Generic;
using System.Text;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Model
{
   public class FingerPrintEntity
    {
        public string FingerPrint { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

        public string Attributes { get; set; }
    }
}
