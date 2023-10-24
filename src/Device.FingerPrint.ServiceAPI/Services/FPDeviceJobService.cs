using Device.FingerPrint.Features.FPDevices.Services;
using Fanex.Logging;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Extensions;
using Newtonsoft.Json;
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
        private readonly ILogger _logger;

        public FPDeviceJobService(IFPDeviceServices deviceService, ILogger logger)
        {
            this.deviceService = deviceService;
            _logger = logger;
        }

        public Task<bool> Execute(int size, int numberOfBatch)
        {
            if (size > 0 && numberOfBatch > 0)
            {
                JobRun(size, numberOfBatch);
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
                            var transIdJson = JsonConvert.SerializeObject(trans);

                            var logMessage = new LogMessage(ex.Message)
                                       .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                                       .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                                       .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                                       .AddMessage(transIdJson)
                                       .ToString();

                            _logger.ErrorAsync(logMessage);
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
