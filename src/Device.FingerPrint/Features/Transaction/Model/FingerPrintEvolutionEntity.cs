using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class FingerPrintEvolutionEntity
    {
        public ulong Id { get; set; }

        public ulong RawFingerPrintId { get; set; }

        public ulong CookieId { get; set; }

        public DateTime TransTime { get; set; }

        public string UserAgent { get; set; }

        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }

        public string Platform { get; set; }

        public int LocalStorage { get; set; }

        public string DoNotTrack { get; set; }

        public int CookiesEnabled { get; set; }

        public string CanvasPrint { get; set; }

        public string Fonts { get; set; }

        public string Vendor { get; set; }

        public string GLRenderer { get; set; }

        public string Plugins { get; set; }

        public string LanguagesPrint { get; set; }

        public string HeaderAccept { get; set; }

        public string HeaderAcceptEncoding { get; set; }

        public string Headers { get; set; }

        public string FPIds { get; set; }
    }
}
