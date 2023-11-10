using Device.FingerPrint.Model;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.IPDB.Services
{
    public interface IIPDBServices
    {
        IEnumerable<IpInfoModel> GetIpInfoList(string ipAddressList);
    }

    public class IPDBServices : IIPDBServices
    {
        private readonly IIPDBDataAccess dataAccess;

        public IPDBServices(IIPDBDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }


        public IEnumerable<IpInfoModel> GetIpInfoList(string ipAddressList)
        {
            return dataAccess.GetIpInfoList(ipAddressList);
        }
    }
}
