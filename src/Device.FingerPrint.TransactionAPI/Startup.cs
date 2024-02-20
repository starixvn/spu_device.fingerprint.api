

namespace Device.FingerPrint.TransactionAPI
{
    using Device.FingerPrint.TransactionAPI.Services;
    using Fanex.Logging.Sentry;
    using Fanex.Logging;
    using MassTransit;
    using ONEbook.UM.Configurations;
    using ONEbook.UM.Helpers;
    using ONEbook.UM.Services;
    using Sentry;
    using StackExchange.Redis.Extensions.Core.Abstractions;
    using StackExchange.Redis.Extensions.Core.Configuration;
    using StackExchange.Redis.Extensions.Core.Implementations;
    using StackExchange.Redis.Extensions.Core;
    using StackExchange.Redis.Extensions.MsgPack;
    using Fanex.LogTrail.Service;
    using Device.FingerPrint.Queue;
    using ONEbook.UM.Transport;
    using Device.FingerPrint.Redis;
    using ONEbook.UM.AspNetCore;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(Configuration);
            services.AddSingleton<IAppSettings>(appSettings);
            services.AddSingleton<ILogEvent>(serviceProvider =>
            {
                var logEvent = new LogEvent(appSettings.LogTrailBaseUrl);
                return logEvent;
            });

            RegisterService(services);
            RegisterRedis(services);

            services.AddControllers();

            LogManager
           .SetDefaultLogCategory("FPS-API")
           .Use(new List<ILogEngine>
           {
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new Dsn(appSettings.SentryDsn)
                    })
           });

            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                busConfigurator.UsingRabbitMq((context, cfx) =>
                {
                    cfx.ConfigureEndpoints(context);
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RegisterService(IServiceCollection services)
        {
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<ISubscriberService, SubscriberService>();
            services.AddSingleton<IMassTransitProducer, MassTransitProducer>();
            services.AddSingleton(new UMClientConfiguration().Initialize(Configuration));
            services.AddSingleton<IUMCacheService, UMCacheService>();
            services.AddSingleton<IUMRestClient, UMRestClient>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
        }

        private void RegisterRedis(IServiceCollection services)
        {
            var redisConfiguration = Configuration.GetSection("Redis").Get<RedisConfiguration>();

            services.AddSingleton(redisConfiguration);

            services.AddSingleton<IRedisClient, RedisClient>();
            services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();
            services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
            services.AddSingleton<IRedisService, RedisService>();
        }
    }
}
