using Device.FingerPrint.Features.Archive.Services;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IArchiveJobServices
    {
        void UpdateModifiedDate(int batchSize);
    }

    public class ArchiveJobServices : IArchiveJobServices
    {
        private readonly IArchiveService archiveService;

        public ArchiveJobServices(IArchiveService archiveService)
        {
            this.archiveService = archiveService;
        }

        public void UpdateModifiedDate(int batchSize)
        {
            archiveService.UpdateModifiedDate(batchSize);
        }
    }
}
