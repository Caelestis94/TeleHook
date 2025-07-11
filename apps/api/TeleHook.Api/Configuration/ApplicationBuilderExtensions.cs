using Microsoft.EntityFrameworkCore;
using Serilog;
using TeleHook.Api.Data;
using TeleHook.Api.Middleware;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Configuration;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigureRequestPipeline(this WebApplication app)
    {
        app.UseCors("AllowAll");
        
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "Host unknown");
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent",
                    httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "User-Agent unknown");
                diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP unknown");
            };
        });

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();
            await next();
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app, string connectionString)
    {
        var logger = Log.ForContext<Program>();
        
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dbPath = connectionString.Replace("Data Source=", "");
            var dbDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
                logger.Information("Created database directory: {Directory}", dbDir);
            }

            await context.Database.MigrateAsync();
            logger.Information("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database migration failed");
            throw;
        }

        return app;
    }

    public static async Task<WebApplication> InitializeTemplateServiceAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var templateService = scope.ServiceProvider.GetRequiredService<ITemplateParsingService>();
        await templateService.InitializeAsync();
        
        return app;
    }

    public static WebApplication AddHealthEndpoint(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        }));

        return app;
    }

    public static void LogStartupInformation(this WebApplication app, string connectionString)
    {
        var logger = Log.ForContext<Program>();
        logger.Information("TeleHook API starting...");
        logger.Information("Environment: {Environment}", app.Environment.EnvironmentName);
        logger.Information("Database Connection: {ConnectionString}",
            connectionString.Contains("Password") ? "[REDACTED]" : connectionString);
        logger.Information("TeleHook API started successfully");
    }
}