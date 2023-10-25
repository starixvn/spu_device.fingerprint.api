using Device.FingerPrint.DCS.QueueConsumer.Handlers;
using Device.FingerPrint.DCS.QueueConsumer.Services;
using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.DCS.QueueConsumer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(this.Configuration);
            services.AddSingleton<IAppSettings>(appSettings);

            services.AddSingleton<ICapturingService, CapturingService>();
            services.AddSingleton<IDeviceDetectorServices, DeviceDetectorServices>();

            services.AddSingleton<ILogger, Logger>();

            services.AddHealthChecks();

            FanexDataConfig(appSettings);

            LogManager
              .SetDefaultLogCategory("Device.FingerPrint.Consumer")
              .Use(new List<ILogEngine>
              {
                        new SentryLogging(new SentryEngineOptions
                        {
                            Dsn = new Dsn(appSettings.SentryDsn)
                        })
              });

            ExceptionHandler
                .UseConfig(new LoggingConfiguration())
                .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);

            services.AddHostedService<Consumer>();
        }
        private void FanexDataConfig(IAppSettings appSettings)
        {
            var connections = Configuration
                                  .GetSection("AppConfig:Connections")
                                  .GetChildren()
                                  .AsEnumerable()
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true
            });
        }
    }
}
