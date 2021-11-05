using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeviceFingerprint.WebAPI.Models.FingerPrint
{
    public class Fonts
    {
        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }

    public class DomBlockers
    {
        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }

    public class Audio
    {
        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class OsCpu
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class Languages
    {
        [JsonProperty("value")]
        public List<List<string>> Value { get; set; }
    }

    public class ColorDepth
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class DeviceMemory
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class HardwareConcurrency
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class Timezone
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class SessionStorage
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class LocalStorage
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class IndexedDB
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class OpenDatabase
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class CpuClass
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class Platform
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class MimeType
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("suffixes")]
        public string Suffixes { get; set; }
    }

    public class Plugins
    {
        [JsonProperty("value")]
        public List<PluginValue> Value { get; set; }
    }

    public class PluginValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("mimeTypes")]
        public List<MimeType> MimeTypes { get; set; }
    }

    public class TouchSupport
    {
        [JsonProperty("value")]
        public TouchSupportValue Value { get; set; }
    }

    public class TouchSupportValue
    {
        [JsonProperty("maxTouchPoints")]
        public int MaxTouchPoints { get; set; }

        [JsonProperty("touchEvent")]
        public bool TouchEvent { get; set; }

        [JsonProperty("touchStart")]
        public bool TouchStart { get; set; }
    }

    public class Vendor
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class VendorFlavors
    {
        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }

    public class CookiesEnabled
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class ColorGamut
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class InvertedColors
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class ForcedColors
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class Monochrome
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class Contrast
    {
        [JsonProperty("value")]
        public ContrastValue Value { get; set; }
    }

    public class ContrastValue
    {
        [JsonProperty("less")]
        public int Less { get; set; }

        [JsonProperty("none")]
        public int None { get; set; }

        [JsonProperty("more")]
        public int More { get; set; }

        [JsonProperty("forcedcolors")]
        public int ForcedColors { get; set; }
    }

    public class ReducedMotion
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class Hdr
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }

    public class Math
    {
        [JsonProperty("value")]
        public MathValue Value { get; set; }
    }

    public class MathValue
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

    public class ScreenPrint
    {
        [JsonProperty("value")]
        public ScreenPrintValue Value { get; set; }
    }

    public class ScreenPrintValue
    {
        [JsonProperty("currentResolution")]
        public string CurrentResolution { get; set; }

        [JsonProperty("pixelDepth")]
        public string PixelDepth { get; set; }
    }

    public class CanvasPrint
    {
        [JsonProperty("value")]
        public CanvasPrintValue Value { get; set; }
    }

    public class CanvasPrintValue
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
        [JsonProperty("value")]
        public FontPreferencePrintValue Value { get; set; }
    }

    public class FontPreferencePrintValue
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

    public class AttributeModel
    {
        [JsonProperty("fonts")]
        public Fonts Fonts { get; set; }

        [JsonProperty("domBlockers")]
        public DomBlockers DomBlockers { get; set; }

        [JsonProperty("audio")]
        public Audio Audio { get; set; }

        [JsonProperty("osCpu")]
        public OsCpu OsCpu { get; set; }

        [JsonProperty("languages")]
        public Languages Languages { get; set; }

        [JsonProperty("colorDepth")]
        public ColorDepth ColorDepth { get; set; }

        [JsonProperty("deviceMemory")]
        public DeviceMemory DeviceMemory { get; set; }

        [JsonProperty("hardwareConcurrency")]
        public HardwareConcurrency HardwareConcurrency { get; set; }

        [JsonProperty("timezone")]
        public Timezone Timezone { get; set; }

        [JsonProperty("sessionStorage")]
        public SessionStorage SessionStorage { get; set; }

        [JsonProperty("localStorage")]
        public LocalStorage LocalStorage { get; set; }

        [JsonProperty("indexedDB")]
        public IndexedDB IndexedDB { get; set; }

        [JsonProperty("openDatabase")]
        public OpenDatabase OpenDatabase { get; set; }

        [JsonProperty("cpuClass")]
        public CpuClass CpuClass { get; set; }

        [JsonProperty("platform")]
        public Platform Platform { get; set; }

        [JsonProperty("plugins")]
        public Plugins Plugins { get; set; }

        [JsonProperty("touchSupport")]
        public TouchSupport TouchSupport { get; set; }

        [JsonProperty("vendor")]
        public Vendor Vendor { get; set; }

        [JsonProperty("vendorFlavors")]
        public VendorFlavors VendorFlavors { get; set; }

        [JsonProperty("cookiesEnabled")]
        public CookiesEnabled CookiesEnabled { get; set; }

        [JsonProperty("colorGamut")]
        public ColorGamut ColorGamut { get; set; }

        [JsonProperty("invertedColors")]
        public InvertedColors InvertedColors { get; set; }

        [JsonProperty("forcedColors")]
        public ForcedColors ForcedColors { get; set; }

        [JsonProperty("monochrome")]
        public Monochrome Monochrome { get; set; }

        [JsonProperty("contrast")]
        public Contrast Contrast { get; set; }

        [JsonProperty("reducedMotion")]
        public ReducedMotion ReducedMotion { get; set; }

        [JsonProperty("hdr")]
        public Hdr Hdr { get; set; }

        [JsonProperty("math")]
        public Math Math { get; set; }

        [JsonProperty("screenPrint")]
        public ScreenPrint ScreenPrint { get; set; }

        [JsonProperty("canvasPrint")]
        public CanvasPrint CanvasPrint { get; set; }

        [JsonProperty("fontPreferencePrint")]
        public FontPreferencePrint FontPreferencePrint { get; set; }
    }


    public class FingerPrintModel
    {
        [JsonProperty("di")]
        public string DI { get; set; }

        [JsonProperty("fk")]
        public string FpsKey { get; set; }

        [JsonProperty("attr")]
        public string Attribute { get; set; }
    }

}
