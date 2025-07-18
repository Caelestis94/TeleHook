using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Configuration;
using TeleHook.Api.DTO;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetDatabaseConnectionString();

builder.Services.AddDatabase(connectionString);

var appSettings = new AppSettingDto();

if (EF.IsDesignTime)
{
    // In design time, we don't load settings from the database
    builder.Services.AddSingleton(new AppSettingDto());
}
else
{
    appSettings = builder.LoadAppSettings();
    builder.Services.AddSingleton(appSettings);
}

builder.Services
    .AddRepositories()
    .AddDomainServices()
    .AddInfrastructureServices()
    .AddBackgroundServices()
    .AddCorsPolicy(builder.Environment)
    .AddApiServices();

builder.Host.ConfigureSerilog(appSettings);

var app = builder.Build();

app.LogStartupInformation(connectionString);

app.ConfigureRequestPipeline();

await app.InitializeDatabaseAsync(connectionString);

await app.InitializeTemplateServiceAsync();

app.AddHealthEndpoint();

app.Run();
