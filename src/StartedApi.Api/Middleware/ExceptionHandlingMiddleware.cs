using System.Net;
using System.Text.Json;
using StartedApi.Application.Common;

namespace StartedApi.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
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
        catch (InvalidOperationException exception)
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
