using Device.FingerPrint.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.RawTransaction.Services
{

    public interface IRawTransactionService
    {
        IEnumerable<RawTransactionModel> GetRawTrans(int batchSize, int retry, string offsetName);
        IEnumerable<RawTransactionModel> GetRawTransN(int batchSize, string offsetName);
    }

    public class RawTransactionService : IRawTransactionService
    {

        private readonly IRawTransactionDataAccess rawTransactionDataAccess;

        public RawTransactionService(IRawTransactionDataAccess rawTransactionDataAccess)
        {
            this.rawTransactionDataAccess = rawTransactionDataAccess;
        }

        public IEnumerable<RawTransactionModel> GetRawTrans(int batchSize, int retry, string offsetName)
        {
            var rawTrans = rawTransactionDataAccess.GetRawTrans(batchSize, retry, offsetName);

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

        public IEnumerable<RawTransactionModel> GetRawTransN(int batchSize, string offsetName)
        {
            var rawTrans = rawTransactionDataAccess.GetRawTransN(batchSize, offsetName);

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
    }
}
