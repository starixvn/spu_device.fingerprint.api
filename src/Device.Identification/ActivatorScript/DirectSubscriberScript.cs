namespace Device.Identification.ActivatorScript
{
    public class DirectSubscriberScript : ISubscriberScript
    {
        public string? ConfigScript { get; set; }

        public string? CommonScript { get; set; }

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
        }
    }
}
