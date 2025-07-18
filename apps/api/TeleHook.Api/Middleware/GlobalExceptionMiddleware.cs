using System.Net;
using System.Text.Json;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;

namespace TeleHook.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation failed";
                response.Details = validationEx.Errors;
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                break;

            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = conflictEx.Message;
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = unauthorizedEx.Message;
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = forbiddenEx.Message;
                break;

            case BadRequestException badRequestEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = badRequestEx.Message;
                response.Details = badRequestEx.Details;
                break;

            case TelegramApiException telegramEx:
                response.StatusCode = (int)HttpStatusCode.BadGateway;
                response.Message = "Telegram API error occurred";
                response.Details = new[] { telegramEx.Message };
                break;

            case PayloadValidationException payloadEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Payload validation failed";
                response.Details = payloadEx.ValidationErrors;
                break;

            case InternalServerErrorException internalServerEx:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = internalServerEx.Message;
                _logger.LogError(internalServerEx, "Internal server error: {Message}", internalServerEx.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal server error occurred";

                // In development, include more details
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    response.Details = new[] { exception.Message, exception.StackTrace };
                }

                if (exception is not BaseException)
                {
                    _logger.LogError(exception, "An unhandled exception occurred while processing the request");
                }

                break;
        }

        context.Response.StatusCode = response.StatusCode;

        if (context.TraceIdentifier != null)
        {
            response.TraceId = context.TraceIdentifier;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
