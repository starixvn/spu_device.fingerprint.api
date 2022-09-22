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
                foreach (var item in fpAttributes)
                {
                    if (await CompareRules(entity, item))
                    {
                        result = item.Id;
                        break;
                    }
                }
            }
            return result;
        }

        private async Task<bool> CompareRules(TransactionEvolutionEntity newEntity, TransactionEvolutionAttributesEntity oldEntity)
        {
            // Rule 1
            if (newEntity.BrowserInstanceCode != oldEntity.BrowserInstanceCode)
                return false;

            // Rule 2
            var version1 = new Version(newEntity.BrowserVersion);
            var version2 = new Version(oldEntity.BrowserVersion);
            var resultVersion = version1.CompareTo(version2);
            if (resultVersion < 0) // new < old
                return false;

            // Rule 3
            var newHashForbiddenChanges = HashForbiddenChanges(newEntity);
            var oldHashForbiddenChanges = HashForbiddenChanges(oldEntity);
            if (newHashForbiddenChanges != oldHashForbiddenChanges)
                return false;

            // Rule 4
            var arr1 = JsonConvert.DeserializeObject<string[]>(newEntity.Fonts);
            var arr2 = JsonConvert.DeserializeObject<string[]>(oldEntity.Fonts);
            if (!arr1.All(arr2.Contains) && !arr2.All(arr1.Contains))
                return false;

            // Rule 5
            int countRule56Changed = 0;

            var listChangesWithSim = GetListAllowChangesWithSim(newEntity, oldEntity);
            if (listChangesWithSim.Count() > maxChangesRule5) return false;

            if (listChangesWithSim.Count() > 0) countRule56Changed++;
            if (!await CheckRatioAllowChangesWithSim(newEntity, oldEntity, listChangesWithSim)) return false;

            // Rule 6
            var listChanges = GetListAllowChanges(newEntity, oldEntity);
            if (listChanges.Count() > maxChangesRule6) return false;
            if (listChanges.Count() > 0) countRule56Changed++;

            // Rule 7
            if (countRule56Changed > 1) return false;

            return true;
        }

        #region "Evolution"
        private string HashForbiddenChanges(TransactionEvolutionAttributesEntity entity)
        {
            var arrForbiddenChanges = Enum.GetValues(typeof(ForbiddenChanges)).Cast<ForbiddenChanges>();
            var hashStr = String.Empty;
            foreach (var item in arrForbiddenChanges)
            {
                var propertyInfo = entity.GetType().GetProperty(item.ToString());
                var value = propertyInfo.GetValue(entity, null);
                hashStr += value.ToString();
            }
            var crypto = new Crypto();
            return crypto.GetMD5Hash(hashStr);
        }

        private IEnumerable<string> GetListAllowChangesWithSim(TransactionEvolutionAttributesEntity newEntity, TransactionEvolutionAttributesEntity oldEntity)
        {
            var result = new List<string>();
            var arrAllowChangesWithSim = Enum.GetValues(typeof(AllowChangesWithSim)).Cast<AllowChangesWithSim>();
            foreach (var item in arrAllowChangesWithSim)
            {
                var key = item.ToString();
                var newValue = GetValueObject(newEntity, key);
                var oldValue = GetValueObject(oldEntity, key);

                if (newValue.ToString() != oldValue.ToString())
                {
                    result.Add(key);
                }

                if (result.Count > maxChangesRule5)
                {
                    break;
                }
            }
            return result;
        }

        private async Task<bool> CheckRatioAllowChangesWithSim(TransactionEvolutionAttributesEntity newEntity, TransactionEvolutionAttributesEntity oldEntity, IEnumerable<string> listKey)
        {
            foreach (var item in listKey)
            {
                var newValue = GetValueObject(newEntity, item);
                var oldValue = GetValueObject(oldEntity, item);

                if (!await CheckRatio(urlRatio, newValue.ToString(), oldValue.ToString()))
                {
                    return false;
                }
            }
            return true;
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
