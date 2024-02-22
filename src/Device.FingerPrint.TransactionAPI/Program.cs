using Device.FingerPrint.TransactionAPI;
using Fanex.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        LogService.Info(string.Format("{0} has been started at {1}", typeof(Program).FullName, DateTimeOffset.Now));

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.SetBasePath(env.ContentRootPath);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                config.AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
#if DEBUG
                webBuilder.UseUrls("http://*:5250");
#else
                    webBuilder.UseUrls("http://*:5000");
#endif

                webBuilder
                .UseStartup<Startup>();
            });
}