using Device.FingerPrint.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.RawTransaction.Services
{

    public interface IRawTransactionService
    {
        IEnumerable<RawTransactionModel> GetRawTrans(RawTransactionConfigModel config);
    }

    public class RawTransactionService : IRawTransactionService
    {

        private readonly IRawTransactionDataAccess rawTransactionDataAccess;

        public RawTransactionService(IRawTransactionDataAccess rawTransactionDataAccess)
        {
            this.rawTransactionDataAccess = rawTransactionDataAccess;
        }

        public IEnumerable<RawTransactionModel> GetRawTrans(RawTransactionConfigModel config)
        {
            var rawTrans = rawTransactionDataAccess.GetRawTrans(config);

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
