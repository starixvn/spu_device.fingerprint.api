using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Device.FingerPrint.ServiceAPI.Services
{
    struct ValuePair
    {
        public object Source;
        public object Destination;
    }

    public interface IEvolutionJobServices
    {
        Task<bool> Evolution(int batchSize, int numberOfBatch);
    }

    public class EvolutionJobServices : IEvolutionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly ISentryClient sentryClient;
        private readonly IAppSettings appSetting;
                
        private readonly int maxChangesRule5 = 2;
        private readonly int maxChangesRule6 = 1;

        public EvolutionJobServices(ITransactionService transactionService, ISentryClient sentryClient, IAppSettings appSetting)
        {
            this.transactionService = transactionService;
            this.sentryClient = sentryClient;
            this.appSetting = appSetting;
        }

        public Task<bool> Evolution(int batchSize, int numberOfBatch)
        {
            try
            {
                var rawTrans = transactionService.GetTransactionEvolution(batchSize * numberOfBatch);

                if (rawTrans != null && rawTrans.Any())
                {
                    ProcessEvolution(rawTrans, batchSize);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private void ProcessEvolution(IEnumerable<TransactionEvolutionEntity> trans, int batchSize)
        {
            if (trans != null && trans.Any())
            {
                var tasks = new List<Task>();
                var chunkRawTrans = trans.Chunk2(batchSize);

                foreach (var item in chunkRawTrans)
                {
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            var (newDeviceTransIds, evolutionModel) = ClassifyRecoverTrans(item);

                            // NewDevice
                            if (newDeviceTransIds != null && newDeviceTransIds.Any())
                            {
                                var transIds = string.Join(",", newDeviceTransIds);
                                transactionService.EvolutionNewDevice(transIds);
                            }

                            // FPS Evolution
                            if (evolutionModel != null && evolutionModel.Any())
                            {
                                var transJson = JsonConvert.SerializeObject(evolutionModel);
                                transactionService.EvolutionRecoverDevice(transJson);
                            }
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + item, SentryLevel.Error);
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private (IEnumerable<ulong>, IEnumerable<TransactionEvolutionModel>) ClassifyRecoverTrans(IEnumerable<TransactionEvolutionEntity> trans)
        {
            var newDeviceTransIds = new List<ulong>();
            var transRecover = new List<TransactionEvolutionModel>();
            foreach (var item in trans)
            {
                var RecoverFingerPrintId = GetRecoverFingerPrintIdByEvolution(item);
                if (RecoverFingerPrintId > 0)
                {
                    var model = new TransactionEvolutionModel()
                    {
                        Id = item.Id,
                        RecoverFingerPrintId = RecoverFingerPrintId
                    };
                    transRecover.Add(model);
                }
                else
                {
                    newDeviceTransIds.Add(item.Id);
                }
            }
            return (newDeviceTransIds, transRecover);
        }

        private ulong GetRecoverFingerPrintIdByEvolution(TransactionEvolutionEntity source)
        {
            ulong result = 0;
            if (source != null && !string.IsNullOrEmpty(source.FPIds))
            {
                var fpAttributes = transactionService.GetTransactionEvolutionAttributes(source.FPIds, source.BrowserInstanceCode);
                foreach (TransactionEvolutionAttributesEntity destination in fpAttributes)
                {
                    if (CompareRules(source, destination))
                    {
                        result = destination.Id;
                        break;
                    }
                }
            }
            return result;
        }

        private bool CompareRules(TransactionEvolutionAttributesEntity source, TransactionEvolutionAttributesEntity destination)
        {
            // Rule 2
            if (!CheckVersion(source, destination)) return false;

            // Rule 3
            if (!CheckForbiddenChanges(source, destination)) return false;

            // Rule 4
            if (!CheckFonts(source, destination)) return false;

            // Rule 5
            int allowChangeWithSimCounter = 0;
            if (!CheckAllowChangesWithSimilar(source, destination, ref allowChangeWithSimCounter)) return false;

            // Rule 6
            int allowChangeCounter = 0;
            if (!CheckAllowChanges(source, destination, ref allowChangeCounter)) return false;

            // Rule 7
            if (allowChangeWithSimCounter != 0 && allowChangeCounter != 0) return false;

            return true;
        }

        #region "Evolution"
        private bool CheckVersion(TransactionEvolutionAttributesEntity source, TransactionEvolutionAttributesEntity destination)
        {
            var version1 = new Version(source.BrowserVersion);
            var version2 = new Version(destination.BrowserVersion);
            var resultVersion = version1.CompareTo(version2);

            if (resultVersion < 0) // new < old
                return false;
            return true;
        }

        private bool CheckFonts(TransactionEvolutionAttributesEntity source, TransactionEvolutionAttributesEntity destination)
        {
            var arr1 = JsonConvert.DeserializeObject<string[]>(source.Fonts);
            var arr2 = JsonConvert.DeserializeObject<string[]>(destination.Fonts);

            if (!arr1.All(arr2.Contains) && !arr2.All(arr1.Contains))
                return false;
            return true;
        }

        private bool CheckForbiddenChanges(TransactionEvolutionAttributesEntity source, TransactionEvolutionAttributesEntity destination)
        {
            var values = new List<ValuePair>
            {
                new ValuePair { Source = source.LocalStorage, Destination = destination.LocalStorage },
                new ValuePair { Source = source.DoNotTrack, Destination = destination.DoNotTrack },
                new ValuePair { Source = source.Cookies, Destination = destination.Cookies },
                new ValuePair { Source = source.Canvas, Destination = destination.Canvas }
            };

            foreach (var value in values)
            {
                if (!value.Source.Equals(value.Destination))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckAllowChangesWithSimilar(TransactionEvolutionAttributesEntity source, TransactionEvolutionAttributesEntity destination, ref int counter)
        {
            var values = new List<ValuePair>
            {
                new ValuePair { Source = source.UserAgent, Destination = destination.UserAgent },
                new ValuePair { Source = source.Vendor, Destination = destination.Vendor },
                new ValuePair { Source = source.GLRenderer, Destination = destination.GLRenderer },
                new ValuePair { Source = source.Plugins, Destination = destination.Plugins },
                new ValuePair { Source = source.Languages, Destination = destination.Languages }
            };

            counter = 0;
            foreach (var value in values)
            {
                if (!value.Source.Equals(value.Destination))
                {
                    var ratioResult = CheckRatio(value.Source.ToString(), value.Destination.ToString()).Result;

                    if (!ratioResult) return false;

                    counter++;
                }

                if (counter > maxChangesRule5)
                    return false;
            }

            return true;

        }

        private bool CheckAllowChanges(TransactionEvolutionAttributesEntity source, TransactionEvolutionAttributesEntity destination, ref int counter)
        {
            var values = new List<ValuePair>
            {
                new ValuePair { Source = source.Resolution, Destination = destination.Resolution },
                new ValuePair { Source = source.Timezone, Destination = destination.Timezone },
                new ValuePair { Source = source.GLRenderer, Destination = destination.GLRenderer }
            };

            counter = 0;
            foreach (var value in values)
            {
                if (!value.Source.Equals(value.Destination))
                {
                    counter++;
                }

                if (counter > maxChangesRule6) return false;
            }

            return true;
        }

        private async Task<bool> CheckRatio(string source, string destination)
        {
            var client = new HttpClient();
            var url = this.appSetting.ApiUrl + "?source=" + HttpUtility.UrlEncode(source) + "&destination=" + HttpUtility.UrlEncode(destination);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var ratio = Decimal.Parse(responseBody.TrimEnd('\n'));

            if (ratio < 75) return false;
            return true;
        }
        #endregion
    }
}
