using Device.FingerPrint.Features.FPDevices.Services;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
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
        private readonly IList<string> ignoreMessages;
        private readonly ISentryClient sentryClient;

        public FPDeviceJobService(IFPDeviceServices deviceService, ISentryClient sentryClient)
        {
            this.deviceService = deviceService;
            this.sentryClient = sentryClient;

            ignoreMessages = new List<string> {
                "Deadlock found when trying to get lock; try restarting transaction"
            };
        }

        public Task<bool> Execute(int size, int numberOfBatch)
        {
            if (size > 0 && numberOfBatch > 0)
            {
                try
                {
                    JobRun(size, numberOfBatch);
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);

                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
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
                                sentryClient.CaptureMessage(ex.Message + "\n" + transIdJson, SentryLevel.Error);
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
