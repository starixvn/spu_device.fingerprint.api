namespace Device.Identification.ActivatorScript
{
    using Microsoft.AspNetCore.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class ApiSubscriberScript : ISubscriberScript
    {
        public string? ConfigScript { get; set; }

        public string? CommonScript { get; set; }

        public string? ApiScript { get; set; }

        public async Task WriteScript(HttpResponse response)
        {
            response.ContentType = "text/javascript";
            var scripts = new StringBuilder();

            if (!string.IsNullOrEmpty(ConfigScript))
            {
                scripts.AppendLine(ConfigScript);
            }

            if (!string.IsNullOrEmpty(CommonScript))
            {
                scripts.AppendLine(CommonScript);
            }

            if (!string.IsNullOrEmpty(ApiScript))
            {
                scripts.AppendLine(ApiScript);
            }

            if (scripts.Length > 0)
            {
                await response.WriteAsync(scripts.ToString());
            }
        }
    }

}