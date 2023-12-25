namespace Device.Fingerprint.WebAPI.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class CanvasPrint
    {
        [JsonPropertyName("winding")]
        public bool Winding { get; set; }

        [JsonPropertyName("geometry")]
        public string Geometry { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class FontPreferencePrint
    {
        [JsonPropertyName("default")]
        public double Default { get; set; }

        [JsonPropertyName("apple")]
        public double Apple { get; set; }

        [JsonPropertyName("serif")]
        public double Serif { get; set; }

        [JsonPropertyName("sans")]
        public double Sans { get; set; }

        [JsonPropertyName("min")]
        public double Min { get; set; }

        [JsonPropertyName("system")]
        public double System { get; set; }
    }

    public class Math
    {
        [JsonPropertyName("acos")]
        public double Acos { get; set; }

        [JsonPropertyName("acosh")]
        public double Acosh { get; set; }

        [JsonPropertyName("acoshPf")]
        public double AcoshPf { get; set; }

        [JsonPropertyName("asin")]
        public double Asin { get; set; }

        [JsonPropertyName("asinh")]
        public double Asinh { get; set; }

        [JsonPropertyName("asinhPf")]
        public double AsinhPf { get; set; }

        [JsonPropertyName("atanh")]
        public double Atanh { get; set; }

        [JsonPropertyName("atanhPf")]
        public double AtanhPf { get; set; }

        [JsonPropertyName("atan")]
        public double Atan { get; set; }

        [JsonPropertyName("sin")]
        public double Sin { get; set; }

        [JsonPropertyName("sinh")]
        public double Sinh { get; set; }

        [JsonPropertyName("sinhPf")]
        public double SinhPf { get; set; }

        [JsonPropertyName("cos")]
        public double Cos { get; set; }

        [JsonPropertyName("cosh")]
        public double Cosh { get; set; }

        [JsonPropertyName("coshPf")]
        public double CoshPf { get; set; }

        [JsonPropertyName("tan")]
        public double Tan { get; set; }

        [JsonPropertyName("tanh")]
        public double Tanh { get; set; }

        [JsonPropertyName("tanhPf")]
        public double TanhPf { get; set; }

        [JsonPropertyName("exp")]
        public double Exp { get; set; }

        [JsonPropertyName("expm1")]
        public double Expm1 { get; set; }

        [JsonPropertyName("expm1Pf")]
        public double Expm1Pf { get; set; }

        [JsonPropertyName("log1p")]
        public double Log1p { get; set; }

        [JsonPropertyName("log1pPf")]
        public double Log1pPf { get; set; }

        [JsonPropertyName("powPI")]
        public double PowPI { get; set; }
    }

    public class MimeType
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("suffixes")]
        public string Suffixes { get; set; }
    }

    public class Plugin
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("mimeTypes")]
        public List<MimeType> MimeTypes { get; set; }
    }

    public class ScreenPrint
    {
        [JsonPropertyName("currentResolution")]
        public string CurrentResolution { get; set; }

        [JsonPropertyName("pixelDepth")]
        public int PixelDepth { get; set; }
    }
    public class LanguagesPrint
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("languages")]
        public string Languages { get; set; }

        [JsonPropertyName("userLanguage")]
        public string UserLanguage { get; set; }

        [JsonPropertyName("browserLanguage")]
        public string BrowserLanguage { get; set; }

        [JsonPropertyName("systemLanguage")]
        public string SystemLanguage { get; set; }
    }

    public class TouchSupport
    {
        [JsonPropertyName("maxTouchPoints")]
        public int MaxTouchPoints { get; set; }

        [JsonPropertyName("touchEvent")]
        public bool TouchEvent { get; set; }

        [JsonPropertyName("touchStart")]
        public bool TouchStart { get; set; }
    }

    public class AttributeModel
    {
        [JsonPropertyName("audio")]
        public double Audio { get; set; }

        [JsonPropertyName("canvasPrint")]
        public CanvasPrint CanvasPrint { get; set; }

        [JsonPropertyName("colorDepth")]
        public int ColorDepth { get; set; }

        [JsonPropertyName("colorGamut")]
        public string ColorGamut { get; set; }

        [JsonPropertyName("contrast")]
        public object Contrast { get; set; }

        [JsonPropertyName("cookiesEnabled")]
        public bool CookiesEnabled { get; set; }

        [JsonPropertyName("cpuClass")]
        public object CpuClass { get; set; }

        [JsonPropertyName("deviceMemory")]
        public object DeviceMemory { get; set; }

        [JsonPropertyName("domBlockers")]
        public object DomBlockers { get; set; }

        [JsonPropertyName("fontPreferencePrint")]
        public FontPreferencePrint FontPreferencePrint { get; set; }

        [JsonPropertyName("fonts")]
        public List<string> Fonts { get; set; }

        [JsonPropertyName("forcedColors")]
        public bool ForcedColors { get; set; }

        [JsonPropertyName("hardwareConcurrency")]
        public int HardwareConcurrency { get; set; }

        [JsonPropertyName("hdr")]
        public object Hdr { get; set; }

        [JsonPropertyName("indexedDB")]
        public bool IndexedDB { get; set; }

        [JsonPropertyName("invertedColors")]
        public object InvertedColors { get; set; }

        [JsonPropertyName("languagesPrint")]
        public LanguagesPrint LanguagesPrint { get; set; }

        [JsonPropertyName("localStorage")]
        public bool LocalStorage { get; set; }

        [JsonPropertyName("math")]
        public Math Math { get; set; }

        [JsonPropertyName("monochrome")]
        public int Monochrome { get; set; }

        [JsonPropertyName("openDatabase")]
        public bool OpenDatabase { get; set; }

        [JsonPropertyName("osCpu")]
        public object OsCpu { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("plugins")]
        public List<Plugin> Plugins { get; set; }

        [JsonPropertyName("reducedMotion")]
        public bool ReducedMotion { get; set; }

        [JsonPropertyName("screenPrint")]
        public ScreenPrint ScreenPrint { get; set; }

        [JsonPropertyName("sessionStorage")]
        public bool SessionStorage { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("touchSupport")]
        public TouchSupport TouchSupport { get; set; }

        [JsonPropertyName("vendor")]
        public string Vendor { get; set; }

        [JsonPropertyName("vendorFlavors")]
        public List<string> VendorFlavors { get; set; }
    }

    public class DefenderAttributesModel
    {
        [JsonPropertyName("cv")]
        public bool CheckCanvasDefender { get; set; }

        [JsonPropertyName("gl")]
        public bool CheckWebGLDefender { get; set; }

        [JsonPropertyName("fd")]
        public bool CheckFontDefender { get; set; }

        [JsonPropertyName("ac")]
        public bool CheckAudioContextDefender { get; set; }
    }
    public class FingerPrintMoreAttributeModel
    {
        [JsonPropertyName("di")]
        public string FingerPrintCode { get; set; }        
    }

    public class FingerPrintModel : FingerPrintMoreAttributeModel
    {
        [JsonPropertyName("subs")]
        public int SubscriberId { get; set; }

        [JsonPropertyName("fk")]
        public string SessionKey { get; set; }

        [JsonPropertyName("ckc")]
        public string CookieCode { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("da")]
        public DefenderAttributesModel DefenderAttributes { get; set; }
    }


    public class FingerPrintAttributesModel : FingerPrintMoreAttributeModel
    {
        [JsonPropertyName("attr")]
        public string Attributes { get; set; }

        [JsonPropertyName("acpt")]
        public string Accept { get; set; }

        [JsonPropertyName("enc")]
        public string Encoding { get; set; }

        [JsonPropertyName("hk")]
        public string HeaderKeys { get; set; }

        [JsonPropertyName("pf")]
        public string Platform { get; set; }
    }
}
