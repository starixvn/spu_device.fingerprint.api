namespace Device.FingerPrint.LogTrail.Consumer
{
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.AspNetCore;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Sentry;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Sentry;
    using Device.FingerPrint.LogTrail.Consumer.Handler;
    using Fanex.Data;
    using Fanex.Data.MySql;

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


            services.AddSingleton<Fanex.Logging.ILogger, Logger>();

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
