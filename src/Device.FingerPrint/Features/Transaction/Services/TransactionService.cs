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

        void DeviceIdentify(string transJson, string offsetName);

        void DeviceIdentifyNoFingerprint(string transJson);

        IEnumerable<DeviceTransactionEntity> GetDeviceIdentify(int batchSize, string offsetName);

        void MergeTransaction(int batchSize);

        void UpdateTransaction(string rawTransJson, int retry, string offsetName, long offsetvalue);

        void UpdateTransactionN(string rawTransJson, string offsetName, long offsetvalue);
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

            transactionDataAccess.InsertJsonTransaction(JsonFingerPrint);
        }

        public void DeviceIdentify(string transJson, string offsetName)
        {
            transactionDataAccess.DeviceIdentify(transJson, offsetName);
        }

        public void DeviceIdentifyNoFingerprint(string transJson)
        {
            transactionDataAccess.DeviceIdentifyNoFingerprint(transJson);
        }

        public IEnumerable<DeviceTransactionEntity> GetDeviceIdentify(int batchSize, string offsetName)
        {
            var rawTrans = transactionDataAccess.GetDeviceIdentify(batchSize, offsetName);

            if (rawTrans != null && rawTrans.Any())
            {
                return rawTrans.Select(x => new DeviceTransactionEntity
                {
                    Id = x.Id,
                    RawTransId = x.RawTransId,
                    CustId = x.CustId,
                    SubAccId = x.SubAccId,
                    LoginName = x.LoginName,
                    SubscriberId = x.SubscriberId,
                    FingerPrintCode = x.FingerPrintCode,
                    CookieId = x.CookieId,
                    TransTime = x.TransTime,
                    ClientIp = x.ClientIp,
                    UserAgent = x.UserAgent,
                    Url = x.Url,
                    Action = x.Action,
                    ModifiedDate = x.ModifiedDate,
                    CreatedDate = x.CreatedDate,
                    TransKey = x.TransKey,
                    Flagged = x.Flagged
                });
            }

            return new List<DeviceTransactionEntity>();
        }

        public void MergeTransaction(int batchSize)
        {
            transactionDataAccess.MergeTransaction(batchSize);
        }

        public void UpdateTransaction(string rawTransJson, int retry, string offsetName, long offsetvalue)
        {
            transactionDataAccess.UpdateTransaction(rawTransJson, retry, offsetName, offsetvalue);
        }

        public void UpdateTransactionN(string rawTransJson, string offsetName, long offsetvalue)
        {
            transactionDataAccess.UpdateTransactionN(rawTransJson, offsetName, offsetvalue);
        }
    }
}
