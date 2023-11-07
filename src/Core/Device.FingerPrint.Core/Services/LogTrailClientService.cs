namespace Device.FingerPrint.Core.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Threading;
    using Device.FingerPrint.Core.Model;

    public interface ILogTrailClientService
    {
        Task PostAsync(LogEventInfo logEventInfo);
    }
    public class LogTrailClientService
    {
        private readonly string LogTrailEndPoint;

        public LogTrailClientService(string LogTrailEndPoint)
        {
            if (string.IsNullOrWhiteSpace(LogTrailEndPoint))
            {
                throw new ArgumentException("Log Trail API URL cannot be null or empty.", nameof(LogTrailEndPoint));
            }

            this.LogTrailEndPoint = LogTrailEndPoint + "/api/job/pushLogTrailEvent";
        }

        public async Task PostAsync(LogEventInfo logEventInfo)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, LogTrailEndPoint))
            {
                var json = JsonSerializer.Serialize(logEventInfo);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken())
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}
