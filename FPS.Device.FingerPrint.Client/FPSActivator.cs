using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FPS.Device.FingerPrint.Client
{
    public interface IFpsActivator
    {
        

        Task PushTransaction(string url, string username, string fpsKey);
    }

    public class FpsActivator : IFpsActivator
    {
        private readonly int subscriber = 0;
        
        private readonly HttpClient httpClient;

        public FpsActivator(int subscriber)
        {
            this.subscriber = subscriber;

            httpClient = new HttpClient();
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
