using System;

namespace Device.FingerPrint.Model
{
    public class TransactionModel
    {
        public int SubscriberId { get; set; }

        public string LoginName { get; set; }        

        public string SessionKey { get; set; }

        public string ClientIP { get; set; }

        public string UserAgent { get; set; }

        public string UserAgentCode { get; set; }           

        public string Url { get; set; }

        public string Action { get; set; }

        public string TransKey { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }

        public DateTime TransTime { get; set; }

        public string CookieCode { get; set; }

        public FingerprintAttributeModel FingerprintAttribute { get; set; }

        public UserAgentInfoModel UserAgentInfo { get; set; }

        public BrowserInfoModel BrowserInfo { get; set; }

        public BrowserInfoAttributeModel BrowserInfoAttribute { get; set; }
    }
}
