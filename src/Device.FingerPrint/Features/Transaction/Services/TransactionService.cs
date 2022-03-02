using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
       
        void DeviceIdentify(string transJson, int groupId);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<long>) GetDeviceIdentify(int batchSize);

        void MergeTransaction(int batchSize);

        void MergeTransactionNew(string transJson);        
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionDataAccess transactionDataAccess;

        public TransactionService(ITransactionDataAccess transactionDataAccess)
        {
            this.transactionDataAccess = transactionDataAccess;
        }

        public void DeviceIdentify(string transJson, int groupId)
        {
            transactionDataAccess.DeviceIdentify(transJson, groupId);
        }

        public void DeviceIdentifyNoFingerprint(string rawTransIds)
        {
            transactionDataAccess.DeviceIdentifyNoFingerprint(rawTransIds);
        }

        public (IEnumerable<DeviceIdentifyEntity>, IEnumerable<long>) GetDeviceIdentify(int batchSize)
        {
            return transactionDataAccess.GetDeviceIdentify(batchSize);            
        }

        public void MergeTransaction(int batchSize)
        {
            transactionDataAccess.MergeTransaction(batchSize);
        }

        public void MergeTransactionNew(string transJson)
        {
            transactionDataAccess.MergeTransactionNew(transJson);
        }
    }
}
