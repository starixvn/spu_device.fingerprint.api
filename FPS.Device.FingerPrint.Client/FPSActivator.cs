using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FPS.Device.FingerPrint.Client
{
    public interface IFPSActivator
    {
        string GenerateRoutingKey();

        Task PushTransaction(string url, string username, string fpsKey);
    }

    public class FPSActivator : IFPSActivator
    {
        private readonly int subscriber = 0;
        private readonly ICores cores;
        private readonly HttpClient httpClient;

        public FPSActivator(int subscriber)
        {
            this.subscriber = subscriber;

            cores = new Cores();
            httpClient = new HttpClient();
        }

        public string GenerateRoutingKey()
        {
            return cores.GenerateRandomKey(subscriber);
        }

        public async Task PushTransaction(string url, string username, string fpsKey)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(fpsKey))
            {
                var data = new Dictionary<string, string>
                {
                    { "username", username },
                    { "fpsKey", fpsKey }
                };

                var result = await httpClient.PostAsJsonAsync(url, data);

                result.EnsureSuccessStatusCode();
                
            }           
        }       
    }
}
