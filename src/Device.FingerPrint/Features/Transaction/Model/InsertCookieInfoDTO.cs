using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class InsertCookieInfoDTO
    {
        public string CookieCode { get; set; }

        public DateTime FirstTransTime { get; set; }
    }
}
