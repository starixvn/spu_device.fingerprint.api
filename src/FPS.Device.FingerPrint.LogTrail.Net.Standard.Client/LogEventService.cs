namespace FPS.Device.FingerPrint.LogTrail.Net.Standard.Client
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public interface ILogEventService
    {
        void InsertLogEvent<T>(T content);
    }

    public class LogEventService : ILogEventService
    {
        private readonly string logTrailEnpoint;

        public LogEventService(string logTrailEnpoint)
        {
            if (string.IsNullOrWhiteSpace(logTrailEnpoint))
            {
                throw new InvalidOperationException("Log Trail API URL is not set.");
            }

            this.logTrailEnpoint = logTrailEnpoint + "/api/job/pushLogTrailEvent";
        }

        public void InsertLogEvent<T>(T content)
        {
            try
            {
                _= PostBasicAsync(content, new CancellationToken());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task PostBasicAsync<T>(T content, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, logTrailEnpoint))
            {
                var json = JsonConvert.SerializeObject(content);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    client.Timeout = TimeSpan.FromSeconds(10);

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}
