using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IDeviceIdentifyJobServices
    {
        public void DeviceIdentify(int batchSize, int numberOfBatch);
    }

    public class DeviceIdentifyJobServices : IDeviceIdentifyJobServices
    {
        private readonly ITransactionService transactionService;

        public DeviceIdentifyJobServices(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        public void DeviceIdentify(int batchSize, int numberOfBatch)
        {
            var rawTrans = transactionService.GetDeviceIdentify(batchSize, numberOfBatch);

            SetDeviceIdentify(rawTrans.Item1, 1);

            SetDeviceIdentify(rawTrans.Item2, 0);

            if (rawTrans.Item3 != null && rawTrans.Item3.Any())
            {
                var rawTransIds = string.Join(",", rawTrans.Item3);

                transactionService.DeviceIdentifyNoFingerprint(rawTransIds);
            }            
        }

        private void SetDeviceIdentify(IEnumerable<DeviceIdentifyEntity> trans, int groupType)
        {
            if (trans != null && trans.Any())
            {
                var groupRawTrans = trans.GroupBy(x => x.BatchNumber);                                

                var tasks = new List<Task>();

                foreach (var group in groupRawTrans)
                {
                    tasks.Add(ProcessDeviceIdentify(group, groupType));
                }

                if (tasks.Any())
                {
                    Task.WaitAll(tasks.ToArray());
                }
            }
        }

        private Task ProcessDeviceIdentify(IEnumerable<DeviceIdentifyEntity> rawTrans, int groupId)
        {
            var rawTransJson = JsonConvert.SerializeObject(rawTrans);

            return Task.Run(() =>
            {
                try
                {
                    transactionService.DeviceIdentify(rawTransJson, groupId);
                }
                catch (Exception ex)
                {
                    LogService.Error(ex.Message + "\n" + rawTransJson, ex);
                    throw;
                }
            });
        }
    }
}
