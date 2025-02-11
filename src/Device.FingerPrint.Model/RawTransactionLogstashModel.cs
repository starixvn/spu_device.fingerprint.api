using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Device.FingerPrint.Model.DTO;
using FileHelpers;

namespace Device.FingerPrint.Model
{
    [DelimitedRecord("|")]
    public class RawTransactionLogstashModel
    {
        public string LoginName { get; set; }

        public string SubscriberName { get; set; }

        public int SubscriberId { get; set; }

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

        [FieldHidden]
        public int TransId { get; set; }

        [FieldConverter(typeof(ConvertBigInteger))]
        public BigInteger FPSTransId { get; set; }

        public string SessionKey { get; set; }

        public string FlaggedCode { get; set; }

        public string BotDetection { get; set; }

        public string BotComponent { get; set; }

        public string Country { get; set; }

        public string CountryCode { get; set; }

        public string Region { get; set; }

        public string City { get; set; }

        public string ISP { get; set; }

        public long IPInfoCode { get; set; }

        public string FPTagging { get; set; }

        public string FPFingerPrintCode { get; set; }

        public string FPFingerPrintExtraCode { get; set; }

        public string FPFingerPrintOldCode { get; set; }

        public string FPUserAgentCode { get; set; }

        public FPFingerprintAttributeModel FPFingerPrintAttribute { get; set; }

        public FPFingerprintAttributeExtraModel FPFingerPrintExtraAttribute { get; set; }

        public string ChallengeCode { get; set; }

        public string RawUserAgent { get; set; }

        public string ClientType { get; set; }

        public string ClientName { get; set; }

        public string ClientVersion { get; set; }

        public string OSName { get; set; }

        public string OSShortName { get; set; }

        public string OSVersion { get; set; }

        public string OSPlatform { get; set; }

        public string Device { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int DeviceType { get; set; }

        public string ChallengeIds { get; set; }

        public string Parameters { get; set; }

        public string Expect { get; set; }

        public string Expected { get; set; }

        public string Actual { get; set; }

        public int? ExeServerTimeInSecond { get; set; }

        public int? ExeClientTimeInSecond { get; set; }

        public int Result { get; set; }

        public string ErrMsg { get; set; }

        public bool? IsIncognitoMode { get; set; }

        public string ScriptVersion { get; set; }

        public string MBDeviceCodeMediaDRM { get; set; }

        public string MBDeviceCodeMachine { get; set; }

        public string MBDeviceCodeTagging { get; set; }

        public bool MBIsMobileApp { get; set; }

        public bool MBIsEmulator { get; set; }

        public string MBModelName { get; set; }

        public string MBManufacturer { get; set; }

        public string MBBrand { get; set; }

        public string MBSDKVersion { get; set; }

        public string MBOS { get; set; }

        public string MBTimezone { get; set; }

        public string MBLanguage { get; set; }

        public string MBCountry { get; set; }
    }
}
