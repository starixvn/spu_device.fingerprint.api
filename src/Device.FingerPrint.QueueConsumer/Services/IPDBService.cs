using Device.FingerPrint.Core;
using Device.FingerPrint.Features.IPDB.Services;
using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface IIPDBService
    {
        void TransformTransactionByIps(IEnumerable<RawTransactionModel> transactions);
    }

    public class IPDBService : IIPDBService
    {
        private readonly IAppSettings appSettings;
        private readonly IIPDBServices ipdbServices;

        public IPDBService(IAppSettings appSettings, IIPDBServices ipdbServices)
        {
            this.appSettings = appSettings;
            this.ipdbServices = ipdbServices;
        }

        public void TransformTransactionByIps(IEnumerable<RawTransactionModel> transactions)
        {
            if (appSettings.IsExtraSub)
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
        }

        private IEnumerable<IpInfoModel> GetIpInfos(IEnumerable<RawTransactionModel> transactions)
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
