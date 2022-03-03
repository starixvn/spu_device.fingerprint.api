using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Device.Fingerprint.WebAPI.Models
{
    public class CanvasPrint
    {
        [JsonProperty("winding")]
        public bool Winding { get; set; }

        [JsonProperty("geometry")]
        public string Geometry { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class FontPreferencePrint
    {
        [JsonProperty("default")]
        public double Default { get; set; }

        [JsonProperty("apple")]
        public double Apple { get; set; }

        [JsonProperty("serif")]
        public double Serif { get; set; }

        [JsonProperty("sans")]
        public double Sans { get; set; }

        [JsonProperty("min")]
        public double Min { get; set; }

        [JsonProperty("system")]
        public double System { get; set; }
    }

    public class Math
    {
        [JsonProperty("acos")]
        public double Acos { get; set; }

        [JsonProperty("acosh")]
        public double Acosh { get; set; }

        [JsonProperty("acoshPf")]
        public double AcoshPf { get; set; }

        [JsonProperty("asin")]
        public double Asin { get; set; }

        [JsonProperty("asinh")]
        public double Asinh { get; set; }

        [JsonProperty("asinhPf")]
        public double AsinhPf { get; set; }

        [JsonProperty("atanh")]
        public double Atanh { get; set; }

        [JsonProperty("atanhPf")]
        public double AtanhPf { get; set; }

        [JsonProperty("atan")]
        public double Atan { get; set; }

        [JsonProperty("sin")]
        public double Sin { get; set; }

        [JsonProperty("sinh")]
        public double Sinh { get; set; }

        [JsonProperty("sinhPf")]
        public double SinhPf { get; set; }

        [JsonProperty("cos")]
        public double Cos { get; set; }

        [JsonProperty("cosh")]
        public double Cosh { get; set; }

        [JsonProperty("coshPf")]
        public double CoshPf { get; set; }

        [JsonProperty("tan")]
        public double Tan { get; set; }

        [JsonProperty("tanh")]
        public double Tanh { get; set; }

        [JsonProperty("tanhPf")]
        public double TanhPf { get; set; }

        [JsonProperty("exp")]
        public double Exp { get; set; }

        [JsonProperty("expm1")]
        public double Expm1 { get; set; }

        [JsonProperty("expm1Pf")]
        public double Expm1Pf { get; set; }

        [JsonProperty("log1p")]
        public double Log1p { get; set; }

        [JsonProperty("log1pPf")]
        public double Log1pPf { get; set; }

        [JsonProperty("powPI")]
        public double PowPI { get; set; }
    }

    public class MimeType
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("suffixes")]
        public string Suffixes { get; set; }
    }

    public class Plugin
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("mimeTypes")]
        public List<MimeType> MimeTypes { get; set; }
    }

    public class ScreenPrint
    {
        [JsonProperty("currentResolution")]
        public string CurrentResolution { get; set; }

        [JsonProperty("pixelDepth")]
        public int PixelDepth { get; set; }
    }
    public class LanguagesPrint
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("languages")]
        public string Languages { get; set; }

        [JsonProperty("userLanguage")]
        public string UserLanguage { get; set; }

        [JsonProperty("browserLanguage")]
        public string BrowserLanguage { get; set; }

        [JsonProperty("systemLanguage")]
        public string SystemLanguage { get; set; }
    }

    public class TouchSupport
    {
        [JsonProperty("maxTouchPoints")]
        public int MaxTouchPoints { get; set; }

        [JsonProperty("touchEvent")]
        public bool TouchEvent { get; set; }

        [JsonProperty("touchStart")]
        public bool TouchStart { get; set; }
    }

    public class AttributeModel
    {
        [JsonProperty("audio")]
        public double Audio { get; set; }

        [JsonProperty("canvasPrint")]
        public CanvasPrint CanvasPrint { get; set; }

        [JsonProperty("colorDepth")]
        public int ColorDepth { get; set; }

        [JsonProperty("colorGamut")]
        public string ColorGamut { get; set; }

        [JsonProperty("contrast")]
        public object Contrast { get; set; }

        [JsonProperty("cookiesEnabled")]
        public bool CookiesEnabled { get; set; }

        [JsonProperty("cpuClass")]
        public object CpuClass { get; set; }

        [JsonProperty("deviceMemory")]
        public object DeviceMemory { get; set; }

        [JsonProperty("domBlockers")]
        public object DomBlockers { get; set; }

        [JsonProperty("fontPreferencePrint")]
        public FontPreferencePrint FontPreferencePrint { get; set; }

        [JsonProperty("fonts")]
        public List<string> Fonts { get; set; }

        [JsonProperty("forcedColors")]
        public bool ForcedColors { get; set; }

        [JsonProperty("hardwareConcurrency")]
        public int HardwareConcurrency { get; set; }

        [JsonProperty("hdr")]
        public object Hdr { get; set; }

        [JsonProperty("indexedDB")]
        public bool IndexedDB { get; set; }

        [JsonProperty("invertedColors")]
        public object InvertedColors { get; set; }

        [JsonProperty("languagesPrint")]
        public LanguagesPrint LanguagesPrint { get; set; }

        [JsonProperty("localStorage")]
        public bool LocalStorage { get; set; }

        [JsonProperty("math")]
        public Math Math { get; set; }

        [JsonProperty("monochrome")]
        public int Monochrome { get; set; }

        [JsonProperty("openDatabase")]
        public bool OpenDatabase { get; set; }

        [JsonProperty("osCpu")]
        public object OsCpu { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("plugins")]
        public List<Plugin> Plugins { get; set; }

        [JsonProperty("reducedMotion")]
        public bool ReducedMotion { get; set; }

        [JsonProperty("screenPrint")]
        public ScreenPrint ScreenPrint { get; set; }

        [JsonProperty("sessionStorage")]
        public bool SessionStorage { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("touchSupport")]
        public TouchSupport TouchSupport { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("vendorFlavors")]
        public List<string> VendorFlavors { get; set; }
    }


    public class FingerPrintModel
    {
        [JsonProperty("subs")]
        public int SubscriberId { get; set; }

        [JsonProperty("di")]
        public string FingerPrintCode { get; set; }

        [JsonProperty("fk")]
        public string SessionKey { get; set; }

        [JsonProperty("attr")]
        public string FingerPrint { get; set; }

        [JsonProperty("ua")]
        public string UserAgent { get; set; }

        [JsonProperty("uaCode")]
        public string UserAgentCode { get; set; }

        [JsonProperty("cookieCode")]
        public string CookieCode { get; set; }

        [JsonProperty("flagged")]
        public string Flagged { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
