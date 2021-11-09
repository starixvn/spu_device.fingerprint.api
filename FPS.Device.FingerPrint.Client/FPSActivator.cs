using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FPS.Device.FingerPrint.Client
{
    public interface IFpsActivator
    {        

        Task PushTransaction(string url, string username, Microsoft.AspNetCore.Http.HttpContext httpContext);
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

        public async Task PushTransaction(string url, string username, Microsoft.AspNetCore.Http.HttpContext httpContext )
        {            

            if (!string.IsNullOrEmpty(username))
            {
                var data = new Dictionary<string, object>
                {
                    { "username", username },
                    { "subscriberId", subscriber },
                    { "fpsKey", httpContext.Request.Form["__fk"].ToString()}
                };

                var result = await httpClient.PostAsJsonAsync(url, data);

                result.EnsureSuccessStatusCode();
                
            }           
        }       
    }
}
