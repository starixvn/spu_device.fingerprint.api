using Device.FingerPrint.Features.RawTransaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.RawTransaction.Services
{

    public interface IRawTransactionService
    {
        IEnumerable<RawTransactionModel> GetRawTransaction(int batchSize, int retry, int offsetId);

        IEnumerable<RawTransactionModel> GetRawTransactionN(int batchSize, int offsetId);

        void InsertRawTransaction(IEnumerable<TransactionEntity> transactions);

        void UpdateRawTransaction(string rawTrans, int retry, int offsetId, long offsetValue);

        void UpdateRawTransactionN(string rawTrans, int offsetId, long offsetValue);
    }

    public class RawTransactionService : IRawTransactionService
    {

        private readonly IRawTransactionDataAccess rawTransactionDataAccess;

        public RawTransactionService(IRawTransactionDataAccess rawTransactionDataAccess)
        {
            this.rawTransactionDataAccess = rawTransactionDataAccess;
        }

        public IEnumerable<RawTransactionModel> GetRawTransaction(int batchSize, int retry, int offsetId)
        {
            var rawTrans = rawTransactionDataAccess.GetRawTrans(batchSize, retry, offsetId);

            if (rawTrans != null && rawTrans.Any())
            {
                return rawTrans.Select(x => new RawTransactionModel
                {
                    Id = x.Id,
                    SessionKey = x.SessionKey,
                    CustId = x.CustId,
                    SubAccId = x.SubAccId,
                    LoginName = x.LoginName,
                    SubscriberId = x.SubscriberId,
                    Transtime = x.Transtime,
                    ClientIP = x.ClientIP,
                    UserAgent = x.UserAgent,
                    Url = x.Url,
                    TransKey = x.TransKey,
                    Action = x.Action,
                    Retry = x.Retry
                });
            }

            return new List<RawTransactionModel>();
        }

        public IEnumerable<RawTransactionModel> GetRawTransactionN(int batchSize, int offsetId)
        {
            var rawTrans = rawTransactionDataAccess.GetRawTransN(batchSize, offsetId);

            if (rawTrans != null && rawTrans.Any())
            {
                return rawTrans.Select(x => new RawTransactionModel
                {
                    Id = x.Id,
                    SessionKey = x.SessionKey,
                    CustId = x.CustId,
                    SubAccId = x.SubAccId,
                    LoginName = x.LoginName,
                    SubscriberId = x.SubscriberId,
                    Transtime = x.Transtime,
                    ClientIP = x.ClientIP,
                    UserAgent = x.UserAgent,
                    Url = x.Url,
                    TransKey = x.TransKey,
                    Action = x.Action,
                    Retry = x.Retry
                });
            }

            return new List<RawTransactionModel>();
        }

        public void InsertRawTransaction(IEnumerable<TransactionEntity> transactions)
        {
            string JsonFingerPrint = JsonConvert.SerializeObject(transactions);

            rawTransactionDataAccess.InsertRawTransaction(JsonFingerPrint);
        }

        public void UpdateRawTransaction(string rawTrans, int retry, int offsetId, long offsetValue)
        {
            rawTransactionDataAccess.UpdateRawTransaction(rawTrans, retry, offsetId, offsetValue);
        }

        public void UpdateRawTransactionN(string rawTrans, int offsetId, long offsetValue)
        {
            rawTransactionDataAccess.UpdateRawTransactionN(rawTrans, offsetId, offsetValue);
        }       
    }
}
