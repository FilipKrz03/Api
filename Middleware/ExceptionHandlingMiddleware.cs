using System.Diagnostics;
using System.Text.Json;

namespace Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var stackTrace = new StackTrace(exception, true);
        var firstFrameWithFile = stackTrace
            .GetFrames()
            ?.FirstOrDefault(frame => !string.IsNullOrWhiteSpace(frame.GetFileName()));

        var fileName = firstFrameWithFile?.GetFileName() ?? "unknown";
        var lineNumber = firstFrameWithFile?.GetFileLineNumber() ?? 0;
        var methodName = firstFrameWithFile?.GetMethod()?.ToString() ?? "unknown";

        var requestHeaders = context.Request.Headers.ToDictionary(
            h => h.Key,
            h => h.Value.ToString());

        logger.LogError(
            exception,
            """
            Unhandled exception captured by middleware.
            TraceId: {TraceId}
            Request: {RequestMethod} {RequestPath}
            QueryString: {QueryString}
            Endpoint: {Endpoint}
            User: {User}
            RemoteIp: {RemoteIp}
            UserAgent: {UserAgent}
            ContentType: {ContentType}
            File: {File}
            Line: {Line}
            Method: {Method}
            Headers: {@Headers}
            FullException: {FullException}
            """,
            context.TraceIdentifier,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.Value,
            context.GetEndpoint()?.DisplayName,
            context.User?.Identity?.Name ?? "anonymous",
            context.Connection.RemoteIpAddress?.ToString(),
            context.Request.Headers.UserAgent.ToString(),
            context.Request.ContentType,
            fileName,
            lineNumber,
            methodName,
            requestHeaders,
            exception.ToString());

        if (context.Response.HasStarted)
        {
            logger.LogWarning("Response has already started, cannot write error response body.");
            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "An unexpected error occurred.",
            traceId = context.TraceIdentifier,
            details = environment.IsDevelopment() ? exception.ToString() : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
