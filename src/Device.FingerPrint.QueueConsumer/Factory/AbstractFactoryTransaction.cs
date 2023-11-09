using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.QueueConsumer.Factory
{
    public abstract class AbstractFactoryTransaction
    {
        public void HandleTransaction(List<RawTransactionModel> transactions)
        {
            try
            {
                if (transactions.Count >= GetBatchThreshold())
                {
                    InsertTransaction(transactions);
                }
                else
                {
                    DelayTask(transactions);
                }
            }
            catch (Exception ex)
            {
                var logMessage = new LogMessage(ex.Message)
                    .AddSection("Data Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex)).ToString();

                LogService.Error(logMessage);
            }
        }

        protected abstract void InsertTransaction(List<RawTransactionModel> transactions);

        protected abstract void DelayTask(List<RawTransactionModel> transactions);
        protected abstract int GetBatchThreshold();

        public void TransformTransactionByIps(List<RawTransactionModel> transactions)
        {
            var ipInfoList = GetIpInfos(transactions);
            if (ipInfoList != null && ipInfoList.Any())
            {
                transactions = transactions.Select(x =>
                {
                    var ipInfo = ipInfoList.FirstOrDefault(y => x.IPId == y.IP);

                    x.Country = ipInfo?.Country;
                    x.CountryCode = ipInfo?.CountryCode;
                    x.Region = ipInfo?.Region;
                    x.City = ipInfo?.City;
                    x.ISP = ipInfo?.ISP;
                    x.IPInfoCode = Cipher.Hashing2Int(string.Join(";", ipInfo?.CountryCode, ipInfo?.Region, ipInfo?.City, ipInfo?.ISP));

                    return x;
                }).ToList();
            }
        }

        private IEnumerable<IpInfoModel> GetIpInfos(List<RawTransactionModel> transactions)
        {
            var ipAddressList = transactions.Where(x => x.IPId != -1 && !x.IsIPV6).Select(x => x.IPId).ToList();
            var ipAddressJoined = string.Join(",", ipAddressList);
            var ipInfoList = GetIpInfoList(ipAddressJoined);
            return ipInfoList;
        }

        private IEnumerable<IpInfoModel> GetIpInfoList(string ipAddressList)
        {
            return null;
            // return commonDataAccess.GetIpInfoList(ipAddressList);
        }
    }
}
