using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rafiq.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace Rafiq.API.Middleware;

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
        _logger.LogError(exception, "Unhandled error occurred while processing request.");

        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException =>
                ((int)HttpStatusCode.BadRequest, "Validation failed", validationException.Errors.Select(x => x.ErrorMessage).ToArray()),
            UnauthorizedException => ((int)HttpStatusCode.Unauthorized, "Unauthorized", [exception.Message]),
            ForbiddenException => ((int)HttpStatusCode.Forbidden, "Forbidden", [exception.Message]),
            NotFoundException => ((int)HttpStatusCode.NotFound, "Not Found", [exception.Message]),
            BadRequestException => ((int)HttpStatusCode.BadRequest, "Bad Request", [exception.Message]),
            AppException => ((int)HttpStatusCode.BadRequest, "Application Error", [exception.Message]),
            _ => ((int)HttpStatusCode.InternalServerError, "Server Error", ["An unexpected error occurred."])
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            title,
            status = statusCode,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
