namespace Device.Identification.ActivatorScript
{
    public interface ISubscriberScript
    {
        Task WriteScript(HttpResponse response);
    }
}
