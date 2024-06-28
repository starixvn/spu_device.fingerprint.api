using Device.FingerPrint.Model;
using FileHelpers;
using System;
using System.Numerics;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class InsertRawTransactionDTO
    {
        public string LoginName { get; set; }

        public string SubscriberName { get; set; }

        [FieldConverter(ConverterKind.Date, "yyyy-MM-dd HH:mm:ss.ffff")]
        public DateTime TransTime { get; set; }

        [FieldConverter(ConverterKind.Date, "yyyy-MM-dd HH:mm:ss.ffff")]
        public DateTime CreatedDate { get; set; }

        public string DeviceCode { get; set; }

        public string FingerprintCode { get; set; }

        public string FingerprintMoreInfo { get; set; }

        public string UserAgent { get; set; }

        public string IP { get; set; }

        public string FakeIP { get; set; }

        [FieldConverter(typeof(ConvertBigInteger))]
        public BigInteger IPId { get; set; }

        [FieldHidden]
        public bool IsIPV6 { get; set; }

        public int PluginID { get; set; }

        public string URL { get; set; }

        public string Action { get; set; }

        public string ActionResult { get; set; }

        public string InvalidDevice { get; set; }

        public int TransStatus { get; set; }

        [FieldConverter(typeof(ConvertBigInteger))]
        public BigInteger FPSTransId { get; set; }

        public string FlaggedCode { get; set; }

        public string BotDetection { get; set; }

        public string BotComponent { get; set; }

        public string CookieCode { get; set; }

        public string RawFingerPrintCode { get; set; }

        public string BrowserInstanceCode { get; set; }

        public string BrowserFamilyCode { get; set; }

        public string ChallengeCode { get; set; }

        public string RawUserAgentCode { get; set; }
    }
}