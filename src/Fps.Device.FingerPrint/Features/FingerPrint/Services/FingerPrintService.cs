using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintService
    {
        void InsertListFingerPrint(List<FingerPrintEntity> listFingerPrint);

        long UpdateCookie(string code);

        void UpdateFingerPrintCookie(FingerPrintCookieEntity fingerprintCookie);
    }

    public class FingerPrintService : IFingerPrintService
    {
        private readonly IFingerPrintDataAccess fingerPrintDataAccess;

        public FingerPrintService(IFingerPrintDataAccess fingerPrintDataAccess)
        {
            this.fingerPrintDataAccess = fingerPrintDataAccess;
        }

        public void InsertListFingerPrint(List<FingerPrintEntity> listFingerPrint)
        {
            string JsonFingerPrint = JsonConvert.SerializeObject(listFingerPrint);
            fingerPrintDataAccess.InsertJsonFingerPrint(JsonFingerPrint);
        }

        public long UpdateCookie(string code)
        {
            return fingerPrintDataAccess.UpdateCookie(code);
        }

        public void UpdateFingerPrintCookie(FingerPrintCookieEntity fingerprintCookie)
        {
            fingerPrintDataAccess.UpdateFingerPrintCookie(fingerprintCookie);
        }
    }
}
