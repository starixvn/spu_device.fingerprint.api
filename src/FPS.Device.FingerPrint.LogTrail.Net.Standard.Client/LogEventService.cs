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
        Task InsertLogEventAsync<T>(T content);
    }

    public class LogEventService : ILogEventService
    {
        private readonly string logTrailEnpoint;

        public LogEventService(string logTrailEnpoint)
        {
            this.logTrailEnpoint = logTrailEnpoint + "/api/job/pushLogTrailEvent";
        }

        public async Task InsertLogEventAsync<T>(T content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(logTrailEnpoint))
                {
                    throw new InvalidOperationException("Log Trail API URL is not set.");
                }

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, logTrailEnpoint))
                {
                    var json = JsonConvert.SerializeObject(content);
                    using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        request.Content = stringContent;

                        client.Timeout = TimeSpan.FromSeconds(5);

                        using (var response =
                            await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken())
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
    }
}
