namespace Device.FingerPrint.LogTrail.Consumer
{
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.AspNetCore;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Sentry;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Sentry;
    using Device.FingerPrint.LogTrail.Consumer.Services;
    using Device.FingerPrint.Features.LogTrail.Services;
    using MassTransit;
    using Device.FingerPrint.LogTrail.Consumer.Consumer;
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

            services.AddSingleton<ILogger, Logger>();

            RegisterServices(services);

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

            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                BusConsumerLogTrailConfiguration(busConfigurator, appSettings);

                if (appSettings.GetValue<bool>("RabbitMQ:LogTrail_Error:Queue-Consume"))
                {
                    BusConsumerLogTrailErrorConfiguration(busConfigurator, appSettings);
                }

                busConfigurator.UsingRabbitMq((context, rgx) =>
                {
                    rgx.ClearSerialization();
                    rgx.UseRawJsonSerializer();

                    rgx.UseRateLimit(20, TimeSpan.FromSeconds(5));

                    rgx.ConfigureEndpoints(context);
                });
            });

            services.AddOptions<RabbitMqTransportOptions>()
            .Configure(options =>
            {
                options.Host = appSettings.RabitMQHost;
                options.Port = appSettings.RabitMQPort;
                options.User = appSettings.RabitMQUserName;
                options.Pass = appSettings.RabitMQPassword;
                options.VHost = appSettings.RabitMQVirtualHost;
            });
        }

        private static void BusConsumerLogTrailConfiguration(IBusRegistrationConfigurator busConfigurator, AppSettings appSettings)
        {
            busConfigurator.AddConsumer<LogTrailConsumer>(cfg =>
            {
                cfg.Options<BatchOptions>(options => options
                .SetMessageLimit(appSettings.GetValue<int>("RabbitMQ:LogTrail:BatchSizeLimit"))
                .SetTimeLimit(s: appSettings.GetValue<int>("RabbitMQ:LogTrail:BatchTimeLimit"))
                .SetConcurrencyLimit(10));

                cfg.UseMessageRetry(retry => retry.Interval(
                    appSettings.GetValue<int>("RabbitMQ:LogTrail:RetryCount"),
                    appSettings.GetValue<int>("RabbitMQ:LogTrail:RetryInterval")
                ));

                cfg.UseInMemoryOutbox();
            }).Endpoint(e =>
            {
                e.ConfigureConsumeTopology = false;
                e.Name = appSettings.GetValue<string>("RabbitMQ:LogTrail:ConsumerName");
                e.PrefetchCount = appSettings.GetValue<int>("RabbitMQ:LogTrail:PrefetchCount");
                e.ConcurrentMessageLimit = appSettings.GetValue<int>("RabbitMQ:LogTrail:ConcurrentMessageLimit");
            });
        }

        private static void BusConsumerLogTrailErrorConfiguration(IBusRegistrationConfigurator busConfigurator, AppSettings appSettings)
        {
            busConfigurator.AddConsumer<LogTrailFaultConsumer>(cfg =>
            {
                cfg.Options<BatchOptions>(options => options
                .SetMessageLimit(appSettings.GetValue<int>("RabbitMQ:LogTrail_Error:BatchSizeLimit"))
                .SetTimeLimit(s: appSettings.GetValue<int>("RabbitMQ:LogTrail_Error:BatchTimeLimit"))
                .SetConcurrencyLimit(10));

                cfg.UseMessageRetry(retry => retry.Interval(
                   appSettings.GetValue<int>("RabbitMQ:LogTrail_Error:RetryCount"),
                   appSettings.GetValue<int>("RabbitMQ:LogTrail_Error:RetryInterval")
                ));

                cfg.UseInMemoryOutbox();
            }).Endpoint(e => e.ConfigureConsumeTopology = false);
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

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<LogTrailConsumer>();

            services.AddSingleton<ICapturingService, CaptureService>();
            services.AddSingleton<ILogTrailServices, LogTrailServices>();
            services.AddSingleton<ILogTrailDataAccess, LogTrailDataAccess>();
        }
    }
}
