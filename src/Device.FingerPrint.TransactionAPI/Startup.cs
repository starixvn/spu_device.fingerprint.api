

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
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Device.FingerPrint.Core.Middleware;
    using Microsoft.OpenApi.Models;
    using Prometheus;

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

            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Transaction API", 
                    Version = "v1",
                    Description = "This is transaction API",
                    Contact = new OpenApiContact()
                    {
                        Name = "Admin",
                        Email = "abc@abc.com"
                    }
                });
            });

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

            services.AddHealthChecks();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Custom Metrics to count requests for each endpoint and the method
            var counter = Metrics.CreateCounter("transaction_api_path_counter", "Counts requests to the Transaction API endpoints", new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint" }
            });
            app.Use((context, next) =>
            {
                counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
                return next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var appSettings = app.ApplicationServices.GetRequiredService<IAppSettings>();

            app.UseRouting();

            // Use the Prometheus middleware
            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {                
                c.SwaggerEndpoint("v1/swagger.json", "Transaction API V1");
            });

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<CustomJWTMiddleware>(appSettings.JWTEnable, appSettings.JWTPassword);

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
