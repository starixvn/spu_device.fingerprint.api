using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.ServiceAPI.Models;
using Device.FingerPrint.ServiceAPI.Ultilities;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IEvolutionJobServices
    {
        Task<bool> Evolution(int batchSize, int numberOfBatch);
    }

    public class EvolutionJobServices : IEvolutionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly ISentryClient sentryClient;

        private readonly string urlRatio = "http://localhost:5555/api/ratio";
        private readonly int maxChangesRule5 = 2;
        private readonly int maxChangesRule6 = 1;

        public EvolutionJobServices(ITransactionService transactionService, ISentryClient sentryClient)
        {
            this.transactionService = transactionService;
            this.sentryClient = sentryClient;
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
                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            var (newDeviceTransIds, evolutionModel) = await ClassifyRecoverTrans(item);

                            // NewDevice
                            if (newDeviceTransIds != null && newDeviceTransIds.Any())
                            {
                                var transIds = string.Join(",", newDeviceTransIds);
                                transactionService.UpdateTransactionEvolution(transIds, (int)TransactionEvolutionType.NewDevice);
                            }

                            // FPS Evolution
                            if (evolutionModel != null && evolutionModel.Any())
                            {
                                var transJson = JsonConvert.SerializeObject(evolutionModel);
                                transactionService.UpdateTransactionEvolution(transJson, (int)TransactionEvolutionType.Recover);
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

        private async Task<(IEnumerable<ulong>, IEnumerable<TransactionEvolutionModel>)> ClassifyRecoverTrans(IEnumerable<TransactionEvolutionEntity> trans)
        {
            var newDeviceTransIds = new List<ulong>();
            var transRecover = new List<TransactionEvolutionModel>();
            foreach (var item in trans)
            {
                var RecoverFingerPrintId = await GetRecoverFingerPrintIdByEvolution(item);
                if (RecoverFingerPrintId > 0)
                {
                    var model = new TransactionEvolutionModel()
                    {
                        Id = item.Id,
                        RawFingerPrintId = item.RawFingerPrintId,
                        CookieId = item.CookieId,
                        TransTime = item.TransTime,
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

        private async Task<ulong> GetRecoverFingerPrintIdByEvolution(TransactionEvolutionEntity entity)
        {
            ulong result = 0;
            if (entity != null && !string.IsNullOrEmpty(entity.FPIds))
            {
                var fpAttributes = transactionService.GetTransactionEvolutionAttributes(entity.FPIds, entity.BrowserInstanceCode);
                foreach (TransactionEvolutionAttributesEntity item in fpAttributes)
                {
                    var joinModel = new TransactionEvolutionAttributesModel { _new = entity, _old = item };
                    var dict = TransportToDictionary(entity, item);
                    if (await CompareRules(joinModel))
                    {
                        result = item.Id;
                        break;
                    }
                }
            }
            return result;
        }

        private Dictionary<string, (string, string)> TransportToDictionary(TransactionEvolutionAttributesEntity newEntity, TransactionEvolutionAttributesEntity oldEntity)
        {
            var dict = new Dictionary<string, (string, string)>();
            var arrProperties = newEntity.GetType().GetProperties();
            foreach(var propertyInfo in arrProperties)
            {
                var newValue = propertyInfo.GetValue(newEntity, null) == null ? String.Empty : propertyInfo.GetValue(newEntity, null).ToString();
                var oldValue = propertyInfo.GetValue(oldEntity, null) == null ? String.Empty : propertyInfo.GetValue(oldEntity, null).ToString();

                dict.Add(propertyInfo.Name, (newValue, oldValue));
            }

            return dict;
        }

        private async Task<bool> CompareRules(TransactionEvolutionAttributesModel model)
        {
            // Rule 1
            if (model._new.BrowserInstanceCode != model._old.BrowserInstanceCode)
                return false;

            // Rule 2
            var version1 = new Version(model._new.BrowserVersion);
            var version2 = new Version(model._old.BrowserVersion);
            var resultVersion = version1.CompareTo(version2);
            if (resultVersion < 0) // new < old
                return false;

            // Rule 3
            if (!CheckRule3(model)) return false;

            // Rule 4
            if (!CheckRule4(model)) return false;

            // Rule 5
            var resultRule5 = await CheckRule5(model);
            if (!resultRule5.Item1) return false;

            // Rule 6
            var resultRule6 = CheckRule6(model);
            if (!resultRule6.Item1) return false;

            // Rule 7
            if ((resultRule5.Item2 ? 1 : 0) + (resultRule6.Item2 ? 1 : 0) > 1) return false;

            return true;
        }

        #region "Evolution"
        private bool CheckRule3(TransactionEvolutionAttributesModel model)
        {
            var forbiddenChanges = new List<(object, object)>
            {
                (model._new.LocalStorage, model._old.LocalStorage),
                (model._new.DoNotTrack, model._old.DoNotTrack),
                (model._new.Cookies, model._old.Cookies),
                (model._new.Canvas, model._old.Canvas)
            };

            var item1 = string.Join(",", forbiddenChanges.Select(x => x.Item1));
            var item2 = string.Join(",", forbiddenChanges.Select(x => x.Item2));

            var crypto = new Crypto();
            var hashItem1 = crypto.GetMD5Hash(item1);
            var hashItem2 = crypto.GetMD5Hash(item2);

            if (hashItem1 != hashItem2)
                return false;
            return true;
        }

        private bool CheckRule4(TransactionEvolutionAttributesModel model)
        {
            var arr1 = JsonConvert.DeserializeObject<string[]>(model._new.Fonts);
            var arr2 = JsonConvert.DeserializeObject<string[]>(model._old.Fonts);

            if (!arr1.All(arr2.Contains) && !arr2.All(arr1.Contains))
                return false;
            return true;
        }

        private async Task<(bool, bool)> CheckRule5(TransactionEvolutionAttributesModel model)
        {
            var allowChangesWithSim = new List<(object, object)>();
            var listChanged = new List<(object, object)>();
            var flagChanged = false;

            allowChangesWithSim.Add((model._new.UserAgent, model._old.UserAgent));
            allowChangesWithSim.Add((model._new.Vendor, model._old.Vendor));
            allowChangesWithSim.Add((model._new.GLRenderer, model._old.GLRenderer));
            allowChangesWithSim.Add((model._new.Plugins, model._old.Plugins));
            allowChangesWithSim.Add((model._new.Languages, model._old.Languages));

            foreach (var item in allowChangesWithSim)
            {
                if (item.Item1 != item.Item2)
                {
                    listChanged.Add(item);
                    flagChanged = true;
                }
                if (listChanged.Count > maxChangesRule5)
                {
                    return (false, flagChanged);
                }
            }

            if (listChanged.Count > maxChangesRule5)
            {
                return (false, flagChanged);
            }

            // Check ratio
            foreach (var item in listChanged)
            {
                if (!await CheckRatio(urlRatio, item.Item1.ToString(), item.Item2.ToString()))
                {
                    return (false, flagChanged);
                }
            }
            return (true, flagChanged);
        }

        private (bool, bool) CheckRule6(TransactionEvolutionAttributesModel model)
        {
            var flagChanged = false;
            var listChanged = new List<(object, object)>();

            var allowChanges = new List<(object, object)>
            {
                (model._new.Resolution, model._old.Resolution),
                (model._new.Timezone, model._old.Timezone),
                (model._new.GLRenderer, model._old.GLRenderer)
            };

            foreach (var item in allowChanges)
            {
                if (item.Item1 != item.Item2)
                {
                    listChanged.Add(item);
                    flagChanged = true;
                }
                if (listChanged.Count > maxChangesRule6)
                {
                    return (false, flagChanged);
                }
            }

            return (true, flagChanged);
        }
        private IEnumerable<string> GetListAllowChanges(TransactionEvolutionAttributesEntity newEntity, TransactionEvolutionAttributesEntity oldEntity)
        {
            var result = new List<string>();
            var arrAllowChanges = Enum.GetValues(typeof(AllowChanges)).Cast<AllowChanges>();
            foreach (var item in arrAllowChanges)
            {
                var key = item.ToString();
                var newValue = GetValueObject(newEntity, key);
                var oldValue = GetValueObject(oldEntity, key);

                if (newValue.ToString() != oldValue.ToString())
                {
                    result.Add(key);
                }

                if (result.Count > maxChangesRule6)
                {
                    break;
                }
            }
            return result;
        }

        private string GetValueObject<T>(T obj, string key)
        {
            var newProperty = obj.GetType().GetProperty(key);
            var newValue = newProperty.GetValue(obj, null);
            return newValue.ToString();
        }

        private async Task<bool> CheckRatio(string url, string source, string destination)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url + "/" + source + "/" + destination);
            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var ratio = Decimal.Parse(responseBody.TrimEnd('\n'));
            if (ratio >= 75) return true;
            return false;
        }
        #endregion
    }
}
