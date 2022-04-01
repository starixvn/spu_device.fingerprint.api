using Device.FingerPrint.Features.FPDevices.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IFPDeviceJobService
    {
        Task<bool> Execute(int size, int numberOfBatch);
    }

    public class FPDeviceJobService : IFPDeviceJobService
    {
        private readonly IFPDeviceServices deviceService;
        private const int delayTime = 100; // milliseconds

        public FPDeviceJobService(IFPDeviceServices deviceService)
        {
            this.deviceService = deviceService;
        }
        public async Task<bool> Execute(int size, int numberOfBatch)
        {
            if (size > 0 && numberOfBatch > 0)
            {
                var task = JobRun(size, numberOfBatch);

                await task;
            }

            return true;
        }
        private async Task JobRun(int size, int numberOfBatch)
        {
            var stagingTransIds = deviceService.GetStagingTransIdForDevice(size, numberOfBatch);

            if (stagingTransIds != null && stagingTransIds.Any())
            {
                var groupTrans = stagingTransIds.GroupBy(x => x.BatchId);
                var tasks = new List<Task>();

                foreach (var trans in groupTrans)
                {
                    var task = Task.Run(() =>
                    {
                        deviceService.UpdateDevices(trans);
                    });

                    await Task.Delay(delayTime);

                    tasks.Add(task);
                }

                var taskCompleted = Task.WhenAll(tasks.ToArray());

                await taskCompleted;
            }
        }
    }
}
