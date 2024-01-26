using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Fanex.Data;
using Fanex.Data.MySql;
using FPS.RawTransaction.Insert;
using FPS.RawTransaction.Insert.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IFileReaderService, FileReaderService>();

        services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
        services.AddSingleton<ITransactionService, TransactionService>();

        services.AddSingleton<BusinessLogicService>();

        FanexDataConfig(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var businessLogicService = serviceProvider.GetService<BusinessLogicService>();

        businessLogicService?.RunBusinessLogic(configuration["FilePath"]);
    }

    private static void FanexDataConfig(IConfiguration configuration)
    {
        var appSettings = new AppSettings(configuration);
        var connections = configuration?
                              .GetSection("AppConfig:Connections")?
                              .GetChildren()?
                              .AsEnumerable()?
                              .ToDictionary(
                                       x => x.GetValue<string>("Name"),
                                       y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
                               );

        DbSettingProviderManager
          .StartNewSession()
          .Use(connections)
          .WithMySql()
          .UseDefaultDbSettingProvider(resourcePath: appSettings.SPName)
          .Run();
    }
}