using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

#if DEBUG
                    webBuilder.UseUrls("http://*:5009");
#else
                    webBuilder.UseUrls("http://*:5000");
#endif
                    webBuilder
                    .UseStartup<Startup>().UseKestrel(opt =>
                    {
                        opt.Limits.MaxRequestBodySize= 737280000;
                        opt.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 2048, gracePeriod: TimeSpan.FromSeconds(60));
                        opt.Limits.KeepAliveTimeout= TimeSpan.FromMinutes(10);
                        opt.Limits.RequestHeadersTimeout= TimeSpan.FromMinutes(10);
                    });
                });
    }
}
