namespace FPS.Device.FingerPrint.LogTrail.Net.Standard.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public interface ILogEventService
    {
        Task InsertLogEventAsync<T>(T content);

        Task InsertBatchLogEventAsync<T>(IEnumerable<T> content);
    }

    public class LogEventService : ILogEventService
    {
        private readonly string logTrailBaseUrl;

        public LogEventService(string logTrailBaseUrl)
        {
            this.logTrailBaseUrl = logTrailBaseUrl;
        }

        public async Task InsertLogEventAsync<T>(T content)
        {
            try
            {
                var url = logTrailBaseUrl + "/api/job/pushLogTrailEvent";
                var json = JsonConvert.SerializeObject(content);

                await SendAsync(json, url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task InsertBatchLogEventAsync<T>(IEnumerable<T> content)
        {
            try
            {
                var url = logTrailBaseUrl + "/api/job/pushMultipleLogTrailEvent";
                var json = JsonConvert.SerializeObject(content);

                await SendAsync(json, url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task SendAsync(string json, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("Log Trail API URL is not set.");
            }

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    client.Timeout = TimeSpan.FromSeconds(10);

                    using (var response =
                        await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken())
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}
