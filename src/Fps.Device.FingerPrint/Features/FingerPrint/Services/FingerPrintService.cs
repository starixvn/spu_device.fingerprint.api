using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using FPS.Device.FingerPrint.Model;
using System.Numerics;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintService
    {
        void InsertFingerPrint(FingerPrintEntity fingerPrint);

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

        public void InsertFingerPrint(FingerPrintEntity fingerPrint)
        {

            fingerPrintDataAccess.InsertFingerPrint(fingerPrint);
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
