using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
       
        void DeviceIdentify(string transJson, int groupType);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity> ,IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch);

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

        public void DeviceIdentify(string transJson, int groupType)
        {
            transactionDataAccess.DeviceIdentify(transJson, groupType);
        }

        public void DeviceIdentifyNoFingerprint(string rawTransIds)
        {
            transactionDataAccess.DeviceIdentifyNoFingerprint(rawTransIds);
        }

        public (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity> ,IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch)
        {
            return transactionDataAccess.GetDeviceIdentify(batchSize, numberOfBatch);            
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
