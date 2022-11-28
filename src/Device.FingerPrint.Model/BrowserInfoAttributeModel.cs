using Newtonsoft.Json;

namespace Device.FingerPrint.Model
{
    public class BrowserInfoAttributeModel
    {
        public string SecChUa { get; set; }

        public string SecChUaPlatform { get; set; }

        public string SecChUaMobile { get; set; }

        public string JsBrowserName { get; set; }

        public string JsOSName { get; set; }


        public string BrowserInfoAttributeCode { get; set; }

        public string BrowserInfoAttributeJson
        {
            get
            {
                return JsonConvert.SerializeObject(new BrowserInfoAttributeProp
                {
                    SecChUa = this.SecChUa,
                    SecChUaPlatform = this.SecChUaPlatform,
                    SecChUaMobile = this.SecChUaMobile,
                    JsBrowserName = this.JsBrowserName,
                    JsOSName = this.JsOSName
                });
            }
        }
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
