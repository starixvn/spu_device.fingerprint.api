using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class DeviceIdentifyEntity
    {
        public long Id { get; set; }

        public string FingerPrintCode { get; set; }

        public long CookieId { get; set; }       

        public DateTime TransTime { get; set; }

        public int GroupId { get; set; }
    }
}
