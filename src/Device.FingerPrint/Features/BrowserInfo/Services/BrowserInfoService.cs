using Device.FingerPrint.Features.BrowserInfo.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.BrowserInfo.Services
{
    public interface IBrowserInfoService
    {
        IEnumerable<BrowserInfoDetectionEntity> GetBrowserInfo(int batchSize);

        IEnumerable<BrowserInfoIdEntity> GetBrowserInfoFlaggedDCS(int batchSize);

        void InsertBrowserInfo(IEnumerable<BrowserInfoEntity> listClientTransaction);

        void UpdateBrowserInfo(IEnumerable<BrowserInfoFlaggedEntity> jsonBrowserInfo);

        void UpdateBrowserInfoFlaggedDCS(IEnumerable<BrowserInfoIdEntity> jsonBrowserInfo);
    }

    public class BrowserInfoService : IBrowserInfoService
    {
        private readonly IBrowserInfoDataAccess browserInfoDataAccess;

        public BrowserInfoService(IBrowserInfoDataAccess browserInfoDataAccess)
        {
            this.browserInfoDataAccess = browserInfoDataAccess;
        }

        public IEnumerable<BrowserInfoDetectionEntity> GetBrowserInfo(int batchSize)
        {
            var userAgent = browserInfoDataAccess.GetBrowserInfo(batchSize);

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
                    OSName = x.OSName
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

        public IEnumerable<BrowserInfoIdEntity> GetBrowserInfoFlaggedDCS(int batchSize)
        {
            var browserInfoFlagged = browserInfoDataAccess.GetBrowserInfoFlaggedDCS(batchSize);

            if (browserInfoFlagged != null && browserInfoFlagged.Any())
            {
                return browserInfoFlagged.Select(x => new BrowserInfoIdEntity
                {
                    Id = x.Id
                });
            }

            return new List<BrowserInfoIdEntity>();
        }

        public void UpdateBrowserInfoFlaggedDCS(IEnumerable<BrowserInfoIdEntity> browserInfoFlagged)
        {
            if (browserInfoFlagged != null && browserInfoFlagged.Any())
            {
                string jsonBrowserInfoFlagged = JsonConvert.SerializeObject(browserInfoFlagged);

                browserInfoDataAccess.UpdateBrowserInfoFlaggedDCS(jsonBrowserInfoFlagged);
            }
        }
    }
}
