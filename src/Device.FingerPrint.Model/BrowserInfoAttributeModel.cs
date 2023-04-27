namespace Device.FingerPrint.Model
{
    public class BrowserInfoAttributeModel : BrowserInfoAttributeProp
    {
        public string BrowserInfoAttributeCode { get; set; }
    }

    public class BrowserInfoAttributeProp
    {
        public string SecChUa { get; set; }

        public string SecChUaPlatform { get; set; }

        public string SecChUaMobile { get; set; }

        public string JsBrowserName { get; set; }

        public string JsOSName { get; set; }
    }
}
