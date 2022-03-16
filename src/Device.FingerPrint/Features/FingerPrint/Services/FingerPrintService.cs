using Device.FingerPrint.Features.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintService
    {
        void InsertClientTransaction(IEnumerable<ClientTransactionEntity> listFingerPrint);
    }

    public class FingerPrintService : IFingerPrintService
    {
        private readonly IFingerPrintDataAccess fingerPrintDataAccess;

        public FingerPrintService(IFingerPrintDataAccess fingerPrintDataAccess)
        {
            this.fingerPrintDataAccess = fingerPrintDataAccess;
        }

        public void InsertClientTransaction(IEnumerable<ClientTransactionEntity> listFingerPrint)
        {
            string JsonFingerPrint = JsonConvert.SerializeObject(listFingerPrint);

            fingerPrintDataAccess.InsertClientTransaction(JsonFingerPrint);
        }
    }
}
