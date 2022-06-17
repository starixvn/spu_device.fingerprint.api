using Device.FingerPrint.Features.ServerTransaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.ServerTransaction.Services
{

    public interface IRawTransactionService
    {
        IEnumerable<ServerTransactionModel> GetServerTransaction(int batchSize, int retry, int offsetId);

        IEnumerable<ServerTransactionModel> GetServerTransactionRetry(int batchSize, int retry, int offsetId);

        IEnumerable<ServerTransactionModel> GetServerTransactionN(int batchSize, int retry, int offsetId);

        void InsertServerTransaction(IEnumerable<TransactionEntity> transactions);

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

        public IEnumerable<ServerTransactionModel> GetServerTransaction(int batchSize, int retry, int offsetId)
        {
            var rawTrans = rawTransactionDataAccess.GetServerTrans(batchSize, retry, offsetId);

            if (rawTrans != null && rawTrans.Any())
            {
                return rawTrans.Select(x => new ServerTransactionModel
                {
                    Id = x.Id,
                    SessionKey = x.SessionKey,
                    CustId = x.CustId,
                    SubAccId = x.SubAccId,
                    LoginName = x.LoginName,
                    SubscriberId = x.SubscriberId,
                    TransTime = x.TransTime,
                    ClientIP = x.ClientIP,
                    UserAgentId = x.UserAgentId,
                    Url = x.Url,
                    TransKey = x.TransKey,
                    Action = x.Action,
                    Retry = x.Retry
                });
            }

            return new List<ServerTransactionModel>();
        }

        public IEnumerable<ServerTransactionModel> GetServerTransactionN(int batchSize, int retry, int offsetId)
        {
            var rawTrans = rawTransactionDataAccess.GetRawTransN(batchSize, retry, offsetId);

            if (rawTrans != null && rawTrans.Any())
            {
                return rawTrans.Select(x => new ServerTransactionModel
                {
                    Id = x.Id,
                    SessionKey = x.SessionKey,
                    CustId = x.CustId,
                    SubAccId = x.SubAccId,
                    LoginName = x.LoginName,
                    SubscriberId = x.SubscriberId,
                    TransTime = x.TransTime,
                    ClientIP = x.ClientIP,
                    UserAgentId = x.UserAgentId,
                    Url = x.Url,
                    TransKey = x.TransKey,
                    Action = x.Action,
                    Retry = x.Retry
                });
            }

            return new List<ServerTransactionModel>();
        }

        public IEnumerable<ServerTransactionModel> GetServerTransactionRetry(int batchSize, int retry, int offsetId)
        {
            var rawTrans = rawTransactionDataAccess.GetServerTransRetry(batchSize, retry, offsetId);

            if (rawTrans != null && rawTrans.Any())
            {
                return rawTrans.Select(x => new ServerTransactionModel
                {
                    Id = x.Id,
                    SessionKey = x.SessionKey,
                    CustId = x.CustId,
                    SubAccId = x.SubAccId,
                    LoginName = x.LoginName,
                    SubscriberId = x.SubscriberId,
                    TransTime = x.TransTime,
                    ClientIP = x.ClientIP,
                    UserAgentId = x.UserAgentId,
                    Url = x.Url,
                    TransKey = x.TransKey,
                    Action = x.Action,
                    Retry = x.Retry
                });
            }

            return new List<ServerTransactionModel>();
        }

        public void InsertServerTransaction(IEnumerable<TransactionEntity> transactions)
        {
            var JsonFingerPrint = JsonConvert.SerializeObject(transactions);
            rawTransactionDataAccess.InsertServerTransaction(JsonFingerPrint);
        }

        public void UpdateRawTransaction(string rawTrans, int retry, int offsetId, long offsetValue)
        {
            rawTransactionDataAccess.UpdateServerTransaction(rawTrans, retry, offsetId, offsetValue);
        }

        public void UpdateRawTransactionN(string rawTrans, int offsetId, long offsetValue)
        {
            rawTransactionDataAccess.UpdateServerTransactionN(rawTrans, offsetId, offsetValue);
        }

    }
}
