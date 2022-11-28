using Newtonsoft.Json;

namespace Device.FingerPrint.Model
{
    public class FingerprintAttributeModel
    {
        public string FingerPrintCode { get; set; }

        public string Attribute { get; set; }

        public string Accept { get; set; }

        public string Encoding { get; set; }

        public string Headers { get; set; }

        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }

        public string UserAgent { get; set; }

        public string Platform { get; set; }


        public string BrowserInstanceCode { get; set; }

        public string BrowserInstanceJson { 
            get
            {
                return JsonConvert.SerializeObject(new BrowserInstanceModel
                {
                    OSName = this.OSName,
                    BrowserName = this.BrowserName,
                    BrowserVersion = this.BrowserVersion,
                    Accept = this.Accept,
                    Encoding = this.Encoding,
                    Headers = this.Headers,
                    UserAgent = this.UserAgent,
                    Platform = this.Platform
                });
            }
        }


        public string BrowserFamilyCode { get; set; }

        public string BrowserFamilyJson
        { 
            get
            {
                return JsonConvert.SerializeObject(new BrowserFamilyModel
                {
                    OSName = this.OSName,
                    BrowserName = this.BrowserName,
                    Platform = this.Platform
                });
            }
        }
    }

    public class BrowserInstanceModel
    {

        public string Accept { get; set; }

        public string Encoding { get; set; }

        public string Headers { get; set; }

        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }

        public string UserAgent { get; set; }

        public string Platform { get; set; }
    }

    public class BrowserFamilyModel
    {
        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string Platform { get; set; }
    }
}
