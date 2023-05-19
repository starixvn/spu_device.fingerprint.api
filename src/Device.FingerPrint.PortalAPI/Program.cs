using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;

namespace Device.FingerPrint.PortalAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(100, 100);

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
                    webBuilder
                    .UseStartup<Startup>();

                    webBuilder.UseKestrel(options =>
                    {

#if DEBUG
                        options.ListenAnyIP(5009);
#else
                        // HTTP 5000
                        options.ListenAnyIP(5000);
#endif
                        options.Limits.MinRequestBodyDataRate =
                                new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(15));

                        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    });
                });
    }
}
