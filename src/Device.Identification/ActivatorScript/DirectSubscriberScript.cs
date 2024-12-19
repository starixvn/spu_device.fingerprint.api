using System.Text;

namespace Device.Identification.ActivatorScript
{
    public class DirectSubscriberScript : ISubscriberScript
    {
        public string? ConfigScript { get; set; }

        public string? CommonScript { get; set; }

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

            if (scripts.Length > 0)
            {
                await response.WriteAsync(scripts.ToString());
            }
        }
    }
}
