namespace Device.Identification.ActivatorScript
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class ApiSubscriberScript : ISubscriberScript
    {
        public string? ConfigScript { get; set; }

        public string? CommonScript { get; set; }

        public string? ApiScript { get; set; }

        public async Task WriteScript(HttpResponse response)
        {
            response.ContentType = "text/javascript";

            if (!string.IsNullOrEmpty(ConfigScript))
            {
                await response.WriteAsync(ConfigScript + "\n");
            }

            if (!string.IsNullOrEmpty(CommonScript))
            {
                await response.WriteAsync(CommonScript + "\n");
            }

            if (!string.IsNullOrEmpty(ApiScript))
            {
                await response.WriteAsync(ApiScript + "\n");
            }
        }
    }

}