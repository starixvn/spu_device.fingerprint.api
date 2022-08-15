using Device.FingerPrint.Features.Archive.Services;
using Sentry;
using System;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IArchiveJobServices
    {
        Task<bool> Execute(int batchSize);
    }

    public class ArchiveJobServices : IArchiveJobServices
    {
        private readonly IArchiveService archiveService;
        private readonly ISentryClient sentryClient;

        public ArchiveJobServices(IArchiveService archiveService, ISentryClient sentryClient)
        {
            this.archiveService = archiveService;
            this.sentryClient = sentryClient;
        }

        public Task<bool> Execute(int batchSize)
        {
            try
            {
                if (batchSize > 0)
                {
                    UpdateModifiedDate(batchSize);
                }
                else
                {
                    sentryClient.CaptureMessage("InValid Batch size");
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        void UpdateModifiedDate(int batchSize)
        {
            archiveService.UpdateModifiedDate(batchSize);
        }
    }
}
