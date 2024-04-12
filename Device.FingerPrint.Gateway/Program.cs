using Device.FingerPrint.Gateway.Model;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Sentry;
using System.Text;
using ILogger = Fanex.Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration)
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    });

var appSettings = new Device.FingerPrint.Gateway.AppSettings(builder.Configuration);
builder.Services.AddSingleton<Device.FingerPrint.Gateway.IAppSettings>(appSettings);
builder.Services.AddSingleton<ILogger, Logger>();

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

//await app.UseOcelot(new OcelotPipelineConfiguration
//{
//    PreQueryStringBuilderMiddleware = async (context, next) =>
//    {
//        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
//        await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
//        var bodyAsText = Encoding.UTF8.GetString(buffer);

//        var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(bodyAsText);

//        context.Request.Headers.Add("x-subscriber", transactionRequest?.Subscriber);

//        //context.Items.UpsertDownstreamRequest(new Ocelot.Request.Middleware.DownstreamRequest(new http)
//        await next.Invoke();
//    }
//});

await app.UseOcelot();

LogService.Info(string.Format("This is Gateway API {0} has been started at {1}", typeof(Program).FullName, DateTimeOffset.Now));

app.Run();
