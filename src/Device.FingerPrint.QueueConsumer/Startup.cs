namespace Device.FingerPrint.QueueConsumer
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Services;
    using Device.FingerPrint.QueueConsumer.Handlers;
    using Device.FingerPrint.QueueConsumer.Services;
    using Device.FingerPrint.Redis;
    using Fanex.Data;
    using Fanex.Data.MySql;
    using Fanex.Data.SqlServer;
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.AspNetCore;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Sentry;
    using Fanex.LogTrail.Service;
    using MassTransit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Sentry;
    using StackExchange.Redis;
    using StackExchange.Redis.Extensions.Core;
    using StackExchange.Redis.Extensions.Core.Abstractions;
    using StackExchange.Redis.Extensions.Core.Configuration;
    using StackExchange.Redis.Extensions.Core.Implementations;
    using StackExchange.Redis.Extensions.MsgPack;
    using System.Collections.Generic;
    using System.Linq;

    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(this.Configuration);

            services.AddSingleton<IAppSettings>(appSettings);
            services.AddSingleton<ILogger, Logger>();

            services.AddHealthChecks();

            services.RegisterService();
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

            services.AddSingleton<ILogEvent>(serviceProvider =>
            {
                var logEvent = new LogEvent(appSettings.LogTrailBaseUrl);
                return logEvent;
            });

            ExceptionHandler
                .UseConfig(new LoggingConfiguration())
                .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);

            RegisterRedis(services);

            InitService(services);

            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                busConfigurator.AddConsumer<Consumer>(cfg =>
                {
                    cfg.Options<BatchOptions>(options => options
                    .SetMessageLimit(appSettings.GetValue<int>("Consumer:Masstransit:BatchSizeLimit"))
                    .SetTimeLimit(s: appSettings.GetValue<int>("Consumer:Masstransit:BatchTimeLimit"))
                    .SetConcurrencyLimit(appSettings.GetValue<int>("Consumer:Masstransit:BatchConcurrencyLimit")));
                }).Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Name = appSettings.GetValue<string>("Consumer:Name");
                    e.PrefetchCount = appSettings.GetValue<int>("Consumer:Masstransit:PrefetchCount");
                });


                busConfigurator.UsingRabbitMq((context, rgx) =>
                {
                    //rgx.ClearSerialization();
                    //rgx.UseRawJsonSerializer();

                    rgx.ConfigureEndpoints(context);
                });
            });

            services.AddOptions<RabbitMqTransportOptions>()
            .Configure(options =>
            {
                options.Host = appSettings.GetValue<string>("RabbitMQ:Host");
                options.Port = appSettings.GetValue<ushort>("RabbitMQ:Port");
                options.User = appSettings.GetValue<string>("RabbitMQ:UserName");
                options.Pass = appSettings.GetValue<string>("RabbitMQ:Password");
                options.VHost = appSettings.GetValue<string>("RabbitMQ:VirtualHost");
            });
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
              .WithSqlServer()
              .UseDefaultDbSettingProvider(resourcePath: appSettings.SPName)
              .Run();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseRouting();
        }

        private void RegisterRedis(IServiceCollection services)
        {
            var redisConfiguration = Configuration.GetSection("Redis").Get<RedisConfiguration>();

            services.AddSingleton(redisConfiguration);

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configOptions = new ConfigurationOptions
                {
                    EndPoints = { $"{redisConfiguration.Hosts[0].Host}:{redisConfiguration.Hosts[0].Port}" },
                    Password = redisConfiguration.Password,
                    Ssl = redisConfiguration.Ssl,
                    ConnectTimeout = redisConfiguration.ConnectTimeout,
                    ConnectRetry = 3,
                    KeepAlive = 180,
                    DefaultDatabase = 0
                };
                return ConnectionMultiplexer.Connect(configOptions);
            });

            services.AddSingleton<IRedisClient, RedisClient>();
            services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();
            services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
            services.AddSingleton<IRedisService, RedisService>();
        }

        private void InitService(IServiceCollection services)
        {
            services.AddSingleton<IRawTransactionService, RawTransactionService>();
            services.AddSingleton<IRawTransactionAttributeService, RawTransactionAttributeService>();
            services.AddSingleton<ICrypto, Crypto>();
        }
    }
}
