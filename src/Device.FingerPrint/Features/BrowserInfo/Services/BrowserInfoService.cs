using Device.FingerPrint.Features.BrowserInfo.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.BrowserInfo.Services
{
    public interface IBrowserInfoService
    {
        IEnumerable<BrowserInfoDetectionEntity> GetBrowserInfo(int batchSize, int offsetId);

        void InsertBrowserInfo(IEnumerable<BrowserInfoEntity> listClientTransaction);

        void UpdateBrowserInfo(IEnumerable<BrowserInfoFlaggedEntity> jsonBrowserInfo);
    }

    public class BrowserInfoService : IBrowserInfoService
    {
        private readonly IBrowserInfoDataAccess browserInfoDataAccess;

        public BrowserInfoService(IBrowserInfoDataAccess browserInfoDataAccess)
        {
            this.browserInfoDataAccess = browserInfoDataAccess;
        }

        public IEnumerable<BrowserInfoDetectionEntity> GetBrowserInfo(int batchSize, int offsetId)
        {
            var userAgent = browserInfoDataAccess.GetBrowserInfo(batchSize, offsetId);

            if (userAgent != null && userAgent.Any())
            {
                return userAgent.Select(x => new BrowserInfoDetectionEntity
                {
                    Id = x.Id,
                    UserAgentId = x.UserAgentId,
                    UserAgent = x.UserAgent,
                    UABrowserName = x.UABrowserName,
                    UAOSName = x.UAOSName,
                    HeadlessScore = x.HeadlessScore,
                    BrowserName = x.BrowserName,
                    OSName = x.OSName,
                    IsParsed = x.IsParsed
                });
            }

            return new List<BrowserInfoDetectionEntity>();
        }

        public void InsertBrowserInfo(IEnumerable<BrowserInfoEntity> listBrowserInfo)
        {
            if (listBrowserInfo != null && listBrowserInfo.Any())
            {
                string JsonBrowserInfo = JsonConvert.SerializeObject(listBrowserInfo);

                browserInfoDataAccess.InsertBrowserInfo(JsonBrowserInfo);
            }
        }

        public void UpdateBrowserInfo(IEnumerable<BrowserInfoFlaggedEntity> browserInfoFlaggedList)
        {
            if (browserInfoFlaggedList != null && browserInfoFlaggedList.Any())
            {
                string jsonBrowserInfo = JsonConvert.SerializeObject(browserInfoFlaggedList);

                browserInfoDataAccess.UpdateBrowserInfo(jsonBrowserInfo);
            }
        }
    }
}
