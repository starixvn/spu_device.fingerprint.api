using System;

namespace Device.FingerPrint.Model
{
    public class TransactionEvolutionModel
    {
        public ulong Id { get; set; }

        public ulong RawFingerPrintId { get; set; }

        public ulong CookieId { get; set; }

        public DateTime TransTime { get; set; }

        public ulong RecoverFingerPrintId { get; set; }
    }
}
