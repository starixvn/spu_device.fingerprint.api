using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Redis;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;
using Sentry;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.MsgPack;
using ILogger = Fanex.Logging.ILogger;
using System.Reflection.PortableExecutable;
using StackExchange.Redis.Extensions.MsgPack;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services
    .AddOcelot(builder.Configuration)    
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    })
    .AddDelegatingHandler<ValidateHandler>(true);

var appSettings = new Device.FingerPrint.Gateway.AppSettings(builder.Configuration);
builder.Services.AddSingleton<Device.FingerPrint.Gateway.IAppSettings>(appSettings);
builder.Services.AddSingleton<ILogger, Logger>();

var redisConfiguration = builder.Configuration.GetSection("Redis").Get<RedisConfiguration>();

builder.Services.AddSingleton(redisConfiguration);

builder.Services.AddSingleton<IRedisClient, RedisClient>();
builder.Services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();
builder.Services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddSingleton<ISubscriberService, SubscriberService>();

LogManager
          .SetDefaultLogCategory("FPS-Gateway-API")
          .Use(new List<ILogEngine>
          {
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new Dsn(appSettings.SentryDsn)
                    })
          });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

});
await app.UseOcelot();

LogService.Info(string.Format("This is Gateway API {0} has been started at {1}", typeof(Program).FullName, DateTimeOffset.Now));

app.Run();
