using System.Diagnostics;

namespace TmsApi.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

        public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        _logger.LogInformation(
            "Request {Method} {Path} [CorrelationId: {CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation(
            "Response {StatusCode} elapsed {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}
