namespace Device.FingerPrint.Core.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Threading;
    using Device.FingerPrint.Core.Model;
    using System.Collections.Generic;

    public interface ILogTrailClientService
    {
        Task PostAsync(LogEventInfo logEventInfo);

        Task PostMultipleAsync(IEnumerable<LogEventInfo> logEventInfos);
    }

    public class LogTrailClientService : ILogTrailClientService
    {
        private string LogTrailBaseUrl;

        public LogTrailClientService()
        {
        }

        public LogTrailClientService(string logTrailBaseUrl)
        {
            SetLogTrailBaseUrl(logTrailBaseUrl);
        }

        public async Task PostAsync(LogEventInfo logEventInfo)
        {
            try
            {
                var url = LogTrailBaseUrl + "/api/job/pushLogTrailEvent";
                var json = JsonSerializer.Serialize(logEventInfo);

                await SendAsync(json, url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task PostMultipleAsync(IEnumerable<LogEventInfo> logEventInfos)
        {
            try
            {
                var url = LogTrailBaseUrl + "/api/job/pushMultipleLogTrailEvent";
                var json = JsonSerializer.Serialize(logEventInfos);

                await SendAsync(json, url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task SendAsync(string json, string url)
        {
            try
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

                        using (var response = await client
                            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken())
                            .ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetLogTrailBaseUrl(string logTrailBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(logTrailBaseUrl))
            {
                throw new ArgumentException("Log Trail API URL cannot be null or empty.", nameof(logTrailBaseUrl));
            }

            LogTrailBaseUrl = logTrailBaseUrl;
        }
    }
}
