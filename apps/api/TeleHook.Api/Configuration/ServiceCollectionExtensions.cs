using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Data;
using TeleHook.Api.Repositories;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services;
using TeleHook.Api.Services.Background;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Infrastructure;
using TeleHook.Api.Services.Interfaces;
using TeleHook.Api.Services.Utilities;

namespace TeleHook.Api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();
        services.AddScoped<IBotRepository, BotRepository>();
        services.AddScoped<IWebhookLogRepository, WebhookLogRepository>();
        services.AddScoped<IWebhookStatRepository, WebhookStatRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IWebhookManagementService, WebhookManagementService>();
        services.AddScoped<IWebhookProcessingService, WebhookProcessingService>();
        services.AddScoped<IBotManagementService, BotManagementService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IPayloadCaptureManagementService, PayloadCaptureManagementService>();
        services.AddScoped<ISettingManagementService, SettingManagementService>();
        services.AddSingleton<ITemplateParsingService, TemplateParsingService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddTransient<IMessageFormattingService, MessageFormattingService>();
        services.AddTransient<ITemplateService, TemplateRenderer>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddSingleton<IPayloadCaptureQueue, PayloadCaptureService>();
        services.AddTransient<ITelegramMessageEscaper, TelegramMessageEscaper>();
        services.AddTransient<IJsonToScribanConverter, JsonToScribanConverter>();
        services.AddHttpClient<ITelegramService, TelegramService>();
        services.AddScoped<IWebhookLoggingService, WebhookLoggingService>();
        services.AddScoped<IWebhookStatService, WebhookStatService>();
        services.AddScoped<IFailureNotificationService, FailureNotificationService>();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<PayloadCaptureCleanupService>();
        services.AddHostedService<WebhookLogCleanupService>();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IWebHostEnvironment environment)
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?.Split(',')
                             ?? ["http://localhost:3000", "http://localhost:3001"];

        services.AddCors(options =>
        {
            if (environment.IsDevelopment())
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            }
            else
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }
        });

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddEndpointsApiExplorer();
        services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        return services;
    }
}
