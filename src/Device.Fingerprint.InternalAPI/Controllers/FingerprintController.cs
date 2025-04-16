using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api/[controller]")]
    public class FingerprintController : Controller
    {
        [HttpGet("GetFingerprint")]
        public ActionResult FingerprintAttributes()
        {
            var model = new List<FPFingerprintAttributeModel> {
                new() {
                    ApplePay = "-1",
                    Architecture = "255",
                    Audio = "124.04347527516074",
                    AudioBaseLatency = "-2",
                    Canvas = "57409f849a0588a90cf83518a7802afd",
                    ColorDepth = "24",
                    ColorGamut = "srgb",
                    Contrast = "0",
                    CookiesEnabled = "true",
                    CpuClass = "undefined",
                    DeviceMemory = "8",
                    DomBlockers = "undefined",
                    DoNotTrack = "null",
                    FontPreferences = "{\"default\":119.45000457763672,\"apple\":119.45000457763672,\"serif\":119.45000457763672,\"sans\":115.2125015258789,\"mono\":97.2125015258789,\"min\":7.474999904632568,\"system\":118.2874984741211}",
                    Fonts = "[\"Calibri\",\"Franklin Gothic\",\"MS UI Gothic\",\"Marlett\",\"Segoe UI Light\"]",
                    ForcedColors = "false",
                    HardwareConcurrency = "20",
                    Hdr = "false",
                    IndexedDB = "true",
                    InvertedColors = "undefined",
                    Languages = "[[\"vi\"]]",
                    LocalStorage = "true",
                    Math = "{\"acos\":1.4473588658278522,\"acosh\":709.889355822726,\"acoshPf\":355.291251501643,\"asin\":0.12343746096704435,\"asinh\":0.881373587019543,\"asinhPf\":0.8813735870195429,\"atanh\":0.5493061443340548,\"atanhPf\":0.5493061443340548,\"atan\":0.4636476090008061,\"sin\":0.8178819121159085,\"sinh\":1.1752011936438014,\"sinhPf\":2.534342107873324,\"cos\":-0.8390715290095377,\"cosh\":1.5430806348152437,\"coshPf\":1.5430806348152437,\"tan\":-1.4214488238747245,\"tanh\":0.7615941559557649,\"tanhPf\":0.7615941559557649,\"exp\":2.718281828459045,\"expm1\":1.718281828459045,\"expm1Pf\":1.718281828459045,\"log1p\":2.3978952727983707,\"log1pPf\":2.3978952727983707,\"powPI\":1.9275814160560206E-50}",
                    Monochrome = "0",
                    OpenDatabase = "false",
                    OsCpu = "undefined",
                    PdfViewerEnabled = "true",
                    Platform = "Win32",
                    Plugins = "[{\"name\":\"PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"Chrome PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"Chromium PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"Microsoft Edge PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"WebKit built-in PDF\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]}]",                    
                    ScreenResolution = "[1920,1080]",
                    SessionStorage = "true",                    
                    Timezone = "Asia/Bangkok",                    
                    TouchSupport = "{\"maxTouchPoints\":0,\"touchEvent\":false,\"touchStart\":false}",
                    PrivateClickMeasurement = "undefined",
                    ReducedMotion = "true",
                    ReducedTransparency = "false",
                    ScreenFrame = "[0,0,50,0]",
                    Vendor = "Google Inc.",
                    VendorFlavors = "[\"chrome\"]",
                    WebGlBasics = "{\"version\":\"WebGL 1.0 (OpenGL ES 2.0 Chromium)\",\"vendor\":\"WebKit\",\"vendorUnmasked\":\"Google Inc. (Intel)\",\"renderer\":\"WebKit WebGL\",\"rendererUnmasked\":\"ANGLE (Intel, Intel(R) UHD Graphics 770 (0x00004680) Direct3D11 vs_5_0 ps_5_0, D3D11)\",\"shadingLanguageVersion\":\"WebGL GLSL ES 1.0 (OpenGL ES GLSL ES 1.0 Chromium)\"}",
                    WebGlExtensions= "1ab46c9f0ef5d2d3c50db14c80d03b9d"
                },                
                new() {
                    ApplePay = "1",
                    Architecture = "64",
                    Audio = "0.5",
                    AudioBaseLatency = "0.1",
                    Canvas = "abc123",
                    ColorDepth = "32",
                    ColorGamut = "p3",
                    Contrast = "1",
                    CookiesEnabled = "false",
                    CpuClass = "x86_64",
                    DeviceMemory = "16",
                    DomBlockers = "true",
                    DoNotTrack = "1",
                    FontPreferences = "{\"default\":100,\"apple\":100,\"serif\":100,\"sans\":100,\"mono\":100}",
                    Fonts = "[\"Arial\",\"Times New Roman\"]",
                    ForcedColors = "true",
                    HardwareConcurrency = "8",
                    Hdr = "true",
                    IndexedDB = "false",
                    InvertedColors = "true",
                    Languages = "[[\"en-US\"]]",
                    LocalStorage = "false",
                    Math = "{\"acos\":0.5}",
                    Monochrome = "1",
                    OpenDatabase = "true",
                    OsCpu = "x86_64",
                    PdfViewerEnabled = "false",
                    Platform = "Linux",
                    Plugins = "[{\"name\":\"Plugin1\",\"description\":\"Description1\"},{\"name\":\"Plugin2\",\"description\":\"Description2\"}]"
                },
                


            };
            
            return Ok(model);
        }
    }
}
