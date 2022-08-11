using Device.FingerPrint.Features.BrowserInfo.Criterias;
using Device.FingerPrint.Features.BrowserInfo.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.BrowserInfo.Services
{
    public interface IBrowserInfoDataAccess
    {
        IEnumerable<BrowserInfoDetectionEntity> GetBrowserInfo(int batchSize);

        void InsertBrowserInfo(string jsonBrowserInfo);

        void UpdateBrowserInfo(string jsonBrowserInfo);
    }

    public class BrowserInfoDataAccess : IBrowserInfoDataAccess
    {
        private readonly IDynamicRepository repository;

        public BrowserInfoDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<BrowserInfoDetectionEntity> GetBrowserInfo(int batchSize)
        {
            var criteria = new GetBrowserInfoCriteria
            {
                Size = batchSize
            };

            return repository.Fetch<BrowserInfoDetectionEntity>(criteria);
        }

        public void InsertBrowserInfo(string jsonBrowserInfo)
        {
            var criteria = new InsertBrowserInfoCriteria
            {
                BrowserInfo = jsonBrowserInfo
            };

            repository.Execute(criteria);
        }

        public void UpdateBrowserInfo(string jsonBrowserInfo)
        {
            var criteria = new UpdateBrowserInfoCriteria
            {
                BrowserInfo = jsonBrowserInfo
            };

            repository.Execute(criteria);
        }
    }
}
