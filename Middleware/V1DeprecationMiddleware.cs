using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace TmsApi.Middleware;

public class V1DeprecationMiddleware(RequestDelegate next)
{
    private static readonly DateTimeOffset SunsetDate = new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var path = context.Request.Path.Value ?? "";
            
            if (context.Request.Path.StartsWithSegments("/api/v1"))
            {
                context.Response.Headers["Deprecation"] = "true";
                context.Response.Headers["Sunset"] = SunsetDate.ToString("R");
                
                // Safely find the successor V2 URL
                var suffix = path.StartsWith("/api/v1", StringComparison.OrdinalIgnoreCase) 
                    ? path[7..] 
                    : "";
                
                context.Response.Headers["Link"] = 
                    $"<{context.Request.Scheme}://{context.Request.Host}/api/v2{suffix}>; rel=\"successor-version\"";
            }
            return Task.CompletedTask;
        });

        await next(context);
    }
}