using TeleHook.Api.Exceptions;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    private const string ApiKeyHeaderName = "X-API-KEY";
    
    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }

        var requiresApiKey = endpoint?.Metadata?.GetMetadata<RequireApiKeyAttribute>() != null;
        var path = context.Request.Path.Value?.ToLower();

        if (path?.StartsWith("/api/") == true && !requiresApiKey)
        {
            requiresApiKey = true;
        }
        

        if (requiresApiKey)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                _logger.LogWarning("API Key authentication failed for {RequestId}", context.TraceIdentifier);
                throw new UnauthorizedException("API Key is required");
            }

            var configuredApiKey = _configuration["Security:ApiKey"];

            if (string.IsNullOrEmpty(configuredApiKey))
            {
                throw new InternalServerErrorException("API Key not configured");
            }

            if (!string.Equals(extractedApiKey, configuredApiKey, StringComparison.Ordinal))
            {
                _logger.LogWarning("API Key authentication failed for {RequestId}", context.TraceIdentifier);
                throw new UnauthorizedException("Invalid API Key");
            }
        }

        await _next(context);
    }
    

}