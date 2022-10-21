using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.ServiceAPI.Ultilities;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
                var recoverFingerPrintId = GetRecoverFingerPrintIdByEvolution(item);

                if (recoverFingerPrintId > 0)
                {
                    var model = new TransactionEvolutionModel()
                    {
                        Id = item.Id,
                        RecoverFingerPrintId = recoverFingerPrintId
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
            try
            {
                var version1 = GetVersion(source.BrowserVersion);
                var version2 = GetVersion(destination.BrowserVersion);

                if (version1 != null && version2 != null)
                {
                    var resultVersion = version1.CompareTo(version2);

                    if (resultVersion < 0) // new < old
                        return false;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureMessage(ex.Message + "\nSource EvolutionId: " + source.Id.ToString() + "\nDestination RawFingerPrintId: " + destination.Id.ToString());
                return false;
            }
        }

        private Version GetVersion(string version)
        {
            if (ValidateVersion(version))
            {
                return Version.Parse(version);
            }
            else
            {
                int dashIndex = version.IndexOf("-");
                if (dashIndex > -1)
                    version = version.Substring(0, dashIndex);

                int parenthesisIndex = version.IndexOf("(");
                if (parenthesisIndex > -1)
                    version = version.Substring(0, parenthesisIndex);

                String[] versionArray = version.Split(".");
                if (versionArray.Length > 4)
                {
                    version = String.Concat(versionArray[0], ".", versionArray[1], ".", versionArray[2], ".", versionArray[3]);
                }

                if (ValidateVersion(version))
                {
                    return Version.Parse(version);
                }
            }
            return null;
        }

        static bool ValidateVersion(string version)
        {
            try
            {
                Version ver = Version.Parse(version);
                return true;
            }
            catch
            {
                return false;
            }
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
            var param = new { source, destination };
            var content = new StringContent(JsonConvert.SerializeObject(param), System.Text.Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var response = await client.PostAsync(this.appSetting.RatioUrl, content);

            if(response.Content != null)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var ratio = Decimal.Parse(responseContent);

                if (ratio < 75) return false;
            }

            return true;
        }
        #endregion
    }
}
