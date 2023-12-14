using Device.FingerPrint.Features.IPDB.Criterias;
using Device.FingerPrint.Model;
using Fanex.Data;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.IPDB.Services
{
    public interface IIPDBDataAccess
    {
        IEnumerable<IpInfoModel> GetIpInfoList(string ipAddressList);
    }

    public class IPDBDataAccess : IIPDBDataAccess
    {
        public IEnumerable<IpInfoModel> GetIpInfoList(string ipAddressList)
        {
            var criteria = new IpInfoCriteria
            {
                IpAddressList = ipAddressList
            };

            using (var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey()))
            {
                return db.Query<IpInfoModel>(criteria);
            }
        }
    }
}
