using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Device.Fingerprint.InsertRawTransaction
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                    webBuilder.UseUrls("http://*:5006");
#else
                    webBuilder.UseUrls("http://*:5000");
#endif 

                    webBuilder
                    .UseSentry()
                    .UseStartup<Startup>();
                });
    }
}
