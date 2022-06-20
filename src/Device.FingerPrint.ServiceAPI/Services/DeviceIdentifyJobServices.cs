using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

                foreach (var group in groupRawTrans)
                {
                    ProcessDeviceIdentify(group, groupType);
                }
            }
        }

        private void ProcessDeviceIdentify(IEnumerable<DeviceIdentifyEntity> rawTrans, int groupType)
        {
            var rawTransJson = JsonConvert.SerializeObject(rawTrans);

            if (groupType == 1)
            {
                transactionService.DeviceIdentify(rawTransJson);
            }
            else if (groupType == 0)
            {
                transactionService.DeviceIdentifyNoGroup(rawTransJson);
            }
        }
    }
}
