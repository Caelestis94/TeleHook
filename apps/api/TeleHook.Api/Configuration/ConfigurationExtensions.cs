using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using TeleHook.Api.Data;
using TeleHook.Api.DTO;
using TeleHook.Api.Models;

namespace TeleHook.Api.Configuration;

public static class ConfigurationExtensions
{
    public static string GetDatabaseConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("DatabaseConnectionString")
               ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
               ?? "Data Source=/data/telehook.db";
    }

    public static AppSettingDto LoadAppSettings(this WebApplicationBuilder builder)
    {
        var settings = new AppSettingDto
        {
            LogLevel = "Warning",
            LogPath = "/app/logs/telehook-.log",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 0,
            StatsDaysInterval = 30
        };

        if (EF.IsDesignTime)
        {
            return settings;
        }

        try
        {
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (context.Database.CanConnect() &&
                context.Model.FindEntityType(typeof(AppSetting)) != null)
            {
                var dbSettings = context.AppSettings.FirstOrDefault(e => e.Id == 1);

                if (dbSettings != null)
                {
                    settings.LogLevel = dbSettings.LogLevel;
                    settings.LogPath = dbSettings.LogPath;
                    settings.LogRetentionDays = dbSettings.LogRetentionDays;
                    settings.EnableWebhookLogging = dbSettings.EnableWebhookLogging;
                    settings.WebhookLogRetentionDays = dbSettings.WebhookLogRetentionDays;
                    settings.StatsDaysInterval = dbSettings.StatsDaysInterval;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load settings from database, using defaults: {ex.Message}");
        }

        var envLogLevel = Environment.GetEnvironmentVariable("SERILOG_LOG_LEVEL");
        if (!string.IsNullOrEmpty(envLogLevel))
            settings.LogLevel = envLogLevel;

        return settings;
    }

    private static LogEventLevel ParseLogLevel(string level) => level switch
    {
        "Trace" => LogEventLevel.Verbose,
        "Debug" => LogEventLevel.Debug,
        "Information" => LogEventLevel.Information,
        "Warning" => LogEventLevel.Warning,
        "Error" => LogEventLevel.Error,
        "Critical" => LogEventLevel.Fatal,
        _ => LogEventLevel.Warning
    };

    public static IHostBuilder ConfigureSerilog(this IHostBuilder host, AppSettingDto appSettings)
    {
        host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
            var level = ParseLogLevel(appSettings.LogLevel);
            configuration.MinimumLevel.Is(level);
        });

        return host;
    }

    public static bool ShouldTrustForwardedHeaders(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("Security:TrustForwardedHeaders", false);
    }

    public static ForwardedHeadersOptions GetForwardedHeadersOptions()
    {
        return new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            KnownProxies = {
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("::1")
            }
        };
    }
}
