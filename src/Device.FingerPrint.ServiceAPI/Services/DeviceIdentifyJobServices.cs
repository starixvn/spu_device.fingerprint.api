using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IDeviceIdentifyJobServices
    {
        Task<bool> DeviceIdentify(int batchSize, int numberOfBatch);
    }

    public class DeviceIdentifyJobServices : IDeviceIdentifyJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly ISentryClient sentryClient;

        public DeviceIdentifyJobServices(ITransactionService transactionService, ISentryClient sentryClient)
        {
            this.transactionService = transactionService;
            this.sentryClient = sentryClient;
        }

        public Task<bool> DeviceIdentify(int batchSize, int numberOfBatch)
        {
            try
            {
                var listRawTrans = new List<Task>();

                var rawTrans = transactionService.GetDeviceIdentify(batchSize, numberOfBatch);

                listRawTrans.Add(Task.Run(() =>
                {
                    try
                    {
                        SetDeviceIdentify(rawTrans.Item1, 1);
                    }
                    catch (Exception ex)
                    {
                        var rawTransIds = string.Join(",", rawTrans.Item1);
                        sentryClient.CaptureMessage(ex.Message + "\n" + rawTransIds, SentryLevel.Error);
                    }
                }));

                listRawTrans.Add(Task.Run(() =>
                {
                    try
                    {
                        SetDeviceIdentify(rawTrans.Item2, 0);
                    }
                    catch (Exception ex)
                    {
                        var rawTransIds = string.Join(",", rawTrans.Item2);
                        sentryClient.CaptureMessage(ex.Message + "\n" + rawTransIds, SentryLevel.Error);
                    }
                }));

                listRawTrans.Add(Task.Run(() =>
                {
                    if (rawTrans.Item3 != null && rawTrans.Item3.Any())
                    {
                        var rawTransIds = string.Join(",", rawTrans.Item3);
                        try
                        {
                            transactionService.DeviceIdentifyNoFingerprint(rawTransIds);
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + rawTransIds, SentryLevel.Error);
                        }
                    }
                }));

                Task.WaitAll(listRawTrans.ToArray());
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
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
                    sentryClient.CaptureMessage(ex.Message + "\n" + rawTransJson, SentryLevel.Error);
                }
            });
        }
    }
}
