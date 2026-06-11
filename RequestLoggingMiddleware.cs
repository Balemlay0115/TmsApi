using System.Diagnostics;

namespace TmsApi;

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
        // Generate correlation ID
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // Set correlation ID header before next middleware
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // Log request entry
        _logger.LogInformation(
            "Request {Method} {Path} [CorrelationId: {CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        // Measure elapsed time
        var stopwatch = Stopwatch.StartNew();

        // Pass control to next middleware
        await _next(context);

        // Stop timing and log response
        stopwatch.Stop();
        _logger.LogInformation(
            "Response {StatusCode} elapsed {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}
