namespace Device.Identification.ActivatorScript
{
    public class TokenSubscriberScript : ISubscriberScript
    {
        public string? TokenScript { get; set; }

        public async Task WriteScript(HttpResponse response)
        {
            response.ContentType = "text/javascript";

            if (!string.IsNullOrEmpty(TokenScript))
            {
                await response.WriteAsync(TokenScript + "\n");
            }
        }
    }
}
