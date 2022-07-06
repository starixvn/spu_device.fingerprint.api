using Device.FingerPrint.Features.FPDevices.Services;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IFPDeviceJobService
    {
        void Execute(int size, int numberOfBatch);
    }

    public class FPDeviceJobService : IFPDeviceJobService
    {
        private readonly IFPDeviceServices deviceService;
        private readonly IList<string> ignoreMessages;

        public FPDeviceJobService(IFPDeviceServices deviceService)
        {
            this.deviceService = deviceService;
            ignoreMessages = new List<string> {
                "Deadlock found when trying to get lock; try restarting transaction"
            };
        }

        public void Execute(int size, int numberOfBatch)
        {
            if (size > 0 && numberOfBatch > 0)
            {
                JobRun(size, numberOfBatch);
            }
        }

        private void JobRun(int size, int numberOfBatch)
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
                        try
                        {
                            deviceService.UpdateDevices(trans);
                        }
                        catch (Exception ex)
                        {
                            if (!ignoreMessages.Contains(ex.Message))
                            {
                                var transIdJson = JsonConvert.SerializeObject(trans);
                                LogService.Error(ex.Message + "\n" + transIdJson, ex);
                            }
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
