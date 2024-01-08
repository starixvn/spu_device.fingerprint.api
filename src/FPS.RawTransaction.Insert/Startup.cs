using Fanex.Data;
using Fanex.Data.MySql;
using FPS.RawTransaction.Insert.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    //public IConfiguration Configuration { get; }

    //public Startup(IConfiguration configuration)
    //{
    //    Configuration = configuration;
    //}

    public void ConfigureServices(IServiceCollection services)
    {
        //services.AddSingleton<IFileReaderService>(new FileReaderService());

        //FanexDataConfig();
    }

    //private void FanexDataConfig()
    //{
    //    var spName = "~/App_Data";
    //    var connections = Configuration?
    //                    .GetSection("AppConfig:Connections")
    //                    .GetChildren()
    //                    .AsEnumerable()
    //                    .ToDictionary(
    //                        x => x.GetValue<string>("Name")!,
    //                        y => new ConnectionConfiguration(
    //                            y.GetValue<string>("Name")!,
    //                            y.GetValue<string>("ConnectionString")!,
    //                            y.GetValue<string>("ProviderName")!
    //                        )
    //                    );

    //    DbSettingProviderManager
    //      .StartNewSession()
    //      .Use(connections)
    //      .WithMySql()
    //      .UseDefaultDbSettingProvider(resourcePath: spName)
    //      .Run();
    //}
}