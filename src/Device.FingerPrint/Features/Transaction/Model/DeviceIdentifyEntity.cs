using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class DeviceIdentifyEntity
    {
        public ulong Id { get; set; }

        public DateTime TransTime { get; set; }

        public ulong RawFingerPrintId { get; set; }

        public ulong CookieId { get; set; }

        public long GroupId { get; set; }

        public int BatchNumber { get; set; }
    }
}
