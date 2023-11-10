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

    public class LogTrailClientService : ILogTrailClientService
    {
        private string LogTrailEndPoint;

        public LogTrailClientService()
        {
        }

        public LogTrailClientService(string logTrailEndPoint)
        {
            SetLogTrailEndPoint(logTrailEndPoint);
        }

        public async Task PostAsync(LogEventInfo logEventInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LogTrailEndPoint))
                {
                    throw new InvalidOperationException("Log Trail API URL is not set.");
                }

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, LogTrailEndPoint))
                {
                    var json = JsonSerializer.Serialize(logEventInfo);
                    using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        request.Content = stringContent;
                        client.Timeout = TimeSpan.FromSeconds(5);

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

        public void SetLogTrailEndPoint(string logTrailEndPoint)
        {
            if (string.IsNullOrWhiteSpace(logTrailEndPoint))
            {
                throw new ArgumentException("Log Trail API URL cannot be null or empty.", nameof(logTrailEndPoint));
            }

            LogTrailEndPoint = logTrailEndPoint + "/api/job/pushLogTrailEvent";
        }
    }
}
