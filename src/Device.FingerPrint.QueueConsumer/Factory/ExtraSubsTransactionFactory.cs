using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Device.FingerPrint.Model;
using Device.FingerPrint.Core;
using System;
using Fanex.Logging;
using System.Linq;
using Device.FingerPrint.Features.IPDB.Services;

namespace Device.FingerPrint.QueueConsumer.Factory
{
    public class ExtraSubsTransactionFactory : AbstractFactoryTransaction
    {
        private Task taskExtraSub = null;
        private readonly CancellationTokenSource tokenSourceExtraSub = new CancellationTokenSource();
        private readonly IAppSettings appSettings;
        private readonly IIPDBServices ipdbServices;

        public ExtraSubsTransactionFactory(IAppSettings appSettings, IIPDBServices ipdbServices)
        {
            this.appSettings = appSettings;
            this.ipdbServices = ipdbServices;
        }

        protected override void InsertTransaction(List<RawTransactionModel> transactions)
        {
            TransformTransactionByIps(transactions);
            //_capturingDataService.InsertTransaction(transactions, true);
            transactions.Clear();
        }

        protected override void DelayTask(List<RawTransactionModel> transactions)
        {
            if (taskExtraSub == null)
            {
                taskExtraSub = Task.Delay(appSettings.Interval).ContinueWith((task) =>
                {
                    try
                    {
                        ProcessDelayedTask(transactions);
                    }
                    catch (OperationCanceledException)
                    {
                        LogService.Info("Task of add transaction extra sub list was canceled.");
                    }
                }, tokenSourceExtraSub.Token);
            }
        }

        private void ProcessDelayedTask(List<RawTransactionModel> transactions)
        {
            if (transactions.Any())
            {
                InsertTransaction(transactions);
            }
            taskExtraSub = null;
        }

        protected override int GetBatchThreshold() => appSettings.TransOfExtraSubsBatch;

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
            return ipdbServices.GetIpInfoList(ipAddressList);
        }
    }
}
