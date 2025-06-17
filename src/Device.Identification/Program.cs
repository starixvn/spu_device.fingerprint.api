using Device.Identification;
using Device.Identification.ActivatorScript;
using Device.Identification.Services;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Sentry;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var appSettings = new AppSettings(builder.Configuration);
builder.Services.AddSingleton<IAppSettings>(appSettings);
builder.Services.AddTransient<JwtToken>();
builder.Services.AddHttpContextAccessor();

var detecasDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ScriptsDetecas");
var detecasProvider = new PhysicalFileProvider(detecasDirectory);
var compositeProvider = new CompositeFileProvider(detecasProvider);
builder.Services.AddSingleton<IFileProvider>(compositeProvider);

builder.Services.AddSingleton<BundleConfig>();
builder.Services.AddSingleton<Fanex.Logging.ILogger, Logger>();
builder.Services.AddSingleton<NumberGenerator>();

builder.Services.AddScoped<IEcService, EcService>();
builder.Services.AddScoped<IActivatorService, ActivatorService>();
builder.Services.AddScoped<HttpCachingWrapper>();
builder.Services.AddScoped<EverCookieCachingWrapper>();

builder.Services.AddTransient<ScriptLoader>();
builder.Services.AddTransient<ActivatorScriptFactory>();
builder.Services.AddTransient<IRecoverDeviceService, RecoverDeviceService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCaching();

builder.Services.AddFanexSql(builder.Configuration);

LogManager.SetDefaultLogCategory("Device.Identification")
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

LogService.Info($"Device.Identification Start! {DateTime.Now}");

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true
});

var bundleConfig = app.Services.GetRequiredService<BundleConfig>();
bundleConfig.EnableOptimization = !app.Environment.IsDevelopment();

//bundleConfig.RegisterBundles();

app.Services.GetRequiredService<IMemoryCache>();
app.Services.GetRequiredService<IFileProvider>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

app.Run();