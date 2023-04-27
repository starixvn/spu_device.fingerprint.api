using Device.FingerPrint.Features.Archive.Services;
using Fanex.Logging;
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
        private readonly ILogger _logger;

        public ArchiveJobServices(IArchiveService archiveService, ILogger logger)
        {
            this.archiveService = archiveService;
            _logger = logger;
        }

        public Task<bool> Execute(int batchSize)
        {
            if (batchSize > 0)
            {
                UpdateModifiedDate(batchSize);
            }
            else
            {
                _logger.Info("InValid Batch size");
            }

            return Task.FromResult(true);
        }

        void UpdateModifiedDate(int batchSize)
        {
            archiveService.UpdateModifiedDate(batchSize);
        }
    }
}
