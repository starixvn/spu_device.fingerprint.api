using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
        void InsertListTransaction(IEnumerable<TransactionEntity> transactions);

        void DeviceIdentify(string transJson, int groupId);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<long>) GetDeviceIdentify(int batchSize);

        void MergeTransaction(int batchSize);

        void MergeTransactionNew(string transJson);

        void UpdateTransaction(string rawTransJson, int retry, int offsetId, long offsetValue);

        void UpdateTransactionN(string rawTransJson, int offsetId, long offsetValue);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionDataAccess transactionDataAccess;

        public TransactionService(ITransactionDataAccess transactionDataAccess)
        {
            this.transactionDataAccess = transactionDataAccess;
        }

        public void InsertListTransaction(IEnumerable<TransactionEntity> transactions)
        {
            string JsonFingerPrint = JsonConvert.SerializeObject(transactions);

            transactionDataAccess.InsertTransaction(JsonFingerPrint);
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

        public void UpdateTransaction(string rawTransJson, int retry, int offsetId, long offsetValue)
        {
            transactionDataAccess.UpdateTransaction(rawTransJson, retry, offsetId, offsetValue);
        }

        public void UpdateTransactionN(string rawTransJson, int offsetId, long offsetValue)
        {
            transactionDataAccess.UpdateTransactionN(rawTransJson, offsetId, offsetValue);
        }
    }
}
