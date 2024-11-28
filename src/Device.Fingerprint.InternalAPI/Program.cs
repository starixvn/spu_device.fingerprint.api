using Device.Fingerprint.InternalAPI.Middleware;
using Device.FingerPrint.Core;
using Device.FingerPrint.Core.Model;
using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sentry;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var env = hostingContext.HostingEnvironment;

        config.SetBasePath(env.ContentRootPath)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();
    });


// Add services to the container.
var appSettings = new AppSettings(builder.Configuration);
var JWTService = new JWTService(appSettings);

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IAppSettings>(appSettings);
builder.Services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
builder.Services.AddSingleton<ITransactionService, TransactionService>();


var rateLimiterConfig = appSettings.GetSection<RateLimiterConfig>("AppConfig:RateLimiter");
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = rateLimiterConfig.PermitLimit;
        options.Window = TimeSpan.FromSeconds(rateLimiterConfig.TimeWindow);
        options.QueueLimit = rateLimiterConfig.QueueLimit;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests.", cancellationToken: cancellationToken);
    };
});

#region Authentication
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = true,
        ValidAudience = appSettings.JWTAudience,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(JWTService.keyJWT)
    };
});
#endregion

builder.Services.AddHealthChecks();
builder.Services.AddResponseCaching();

FanexDataConfig(builder.Configuration, appSettings);

LogManager.SetDefaultLogCategory("Device.Fingerprint.InternalAPI")
        .Use([
            new SentryLogging(new SentryEngineOptions
            {
                Dsn = new Dsn(appSettings.SentryDsn)
            })
        ]);

ExceptionHandler
    .UseConfig(new LoggingConfiguration())
    .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);

LogService.Info("InternalAPI Start!");


#region Swagger Configuration
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "Device.Fingerprint.InternalAPI", Version = "v1" });
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

});
#endregion

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true
});

app.UseRouting();

app.UseCors();

app.UseResponseCaching();

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<JWTValidationMiddleware>();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();

static void FanexDataConfig(ConfigurationManager configuration, AppSettings appSettings)
{
    var connections = configuration
                    .GetSection("AppConfig:Connections")
                    .GetChildren()
                    .AsEnumerable()
                    .ToDictionary(
                        x => x.GetValue<string>("Name") ?? string.Empty,
                        y => new ConnectionConfiguration(
                            y.GetValue<string>("Name") ?? string.Empty,
                            y.GetValue<string>("ConnectionString") ?? string.Empty,
                            y.GetValue<string>("ProviderName") ?? string.Empty
                        )
                    );

    DbSettingProviderManager
      .StartNewSession()
      .Use(connections)
      .WithMySql()
      .UseDefaultDbSettingProvider(resourcePath: appSettings.SPName)
      .Run();
}