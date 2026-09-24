using System.Net;
using AIHealthcareAssistant.Application.Common.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AIHealthcareAssistant.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
public ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, exception);
        }
    }

    public static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentException ex =>
                (HttpStatusCode.BadRequest, ex.Message),

            KeyNotFoundException ex =>
                (HttpStatusCode.NotFound, ex.Message),

            UnauthorizedAccessException ex =>
                (HttpStatusCode.Unauthorized, ex.Message),

            InvalidOperationException ex =>
                (HttpStatusCode.Conflict, ex.Message),

            TimeoutException =>
                (HttpStatusCode.RequestTimeout, "The request timed out."),

            HttpRequestException =>
                (HttpStatusCode.ServiceUnavailable,
                    "The requested service is currently unavailable."),

            SecurityTokenException =>
                (HttpStatusCode.Unauthorized,
                    "Invalid or expired token."),

            _ =>
                (HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiErrorResponse.Error(message);

        await context.Response.WriteAsJsonAsync(response);
    }


}
