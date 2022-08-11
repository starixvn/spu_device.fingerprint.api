namespace Device.FingerPrint.JobManagement.Services
{
    using Refit;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IJobService
    {
        Task<bool> Execute(string jobName, string apiUrl, string methodName, string controller, Dictionary<string, object> data);
    }

    public class JobService : IJobService
    { 
        public Task<bool> Execute(string jobName, string apiUrl, string methodName, string controller, Dictionary<string, object> data)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl),
                Timeout = TimeSpan.FromDays(1)
            };

            var jobServiceAPI = RestService.For<IJobServiceApi>(httpClient);            

            return jobServiceAPI.Execute(jobName, methodName, controller, data);
        }
    }
}
