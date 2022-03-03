using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintService
    {
        void InsertListFingerPrint(IEnumerable<FingerPrintEntity> listFingerPrint);

        void UpdateFingerPrintCookie(FingerPrintCookieEntity fingerprintCookie);
    }

    public class FingerPrintService : IFingerPrintService
    {
        private readonly IFingerPrintDataAccess fingerPrintDataAccess;

        public FingerPrintService(IFingerPrintDataAccess fingerPrintDataAccess)
        {
            this.fingerPrintDataAccess = fingerPrintDataAccess;
        }

        public void InsertListFingerPrint(IEnumerable<FingerPrintEntity> listFingerPrint)
        {
            string JsonFingerPrint = JsonConvert.SerializeObject(listFingerPrint);

            fingerPrintDataAccess.InsertJsonFingerPrint(JsonFingerPrint);
        }

        public void UpdateFingerPrintCookie(FingerPrintCookieEntity fingerprintCookie)
        {
            fingerPrintDataAccess.UpdateFingerPrintCookie(fingerprintCookie);
        }
    }
}
