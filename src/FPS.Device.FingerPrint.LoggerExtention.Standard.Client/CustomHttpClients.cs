namespace FPS.Device.FingerPrint.LoggerExtention.Standard.Client
{
    using System.IO;
    using System.Net.Http;
    using Serilog.Sinks.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using System;

    public class CustomHttpClients : IHttpClient
    {
        private readonly HttpClient httpClient;
        private IConfiguration configuration;

        public CustomHttpClients() => httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            try
            {
                var content = new StreamContent(contentStream);
                content.Headers.Add("Content-Type", "application/json");

                var response = await httpClient
                    .PostAsync(requestUri, content)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose() => httpClient?.Dispose();

        public void Configure(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
