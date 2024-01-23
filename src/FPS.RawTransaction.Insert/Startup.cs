using Device.FingerPrint.Features.Subscriber.Services;
using Fanex.Data;
using Fanex.Data.MySql;
using FPS.RawTransaction.Insert.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISubscriberDataAccess, SubscriberDataAccess>();
        services.AddSingleton<ISubscriberServices, SubscriberServices>();
    }
}