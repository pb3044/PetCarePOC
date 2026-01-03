using System.Collections.Concurrent;
using System.Net;

namespace PetCarePlatform.Web.Middleware
{
    /// <summary>
    /// Simple rate limiting middleware to prevent abuse.
    /// For production, consider using AspNetCoreRateLimit package.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            // Default: 100 requests per 60 seconds
            _maxRequests = 100;
            _timeWindow = TimeSpan.FromSeconds(60);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for health checks
            if (context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            var clientIp = GetClientIpAddress(context);
            var key = $"{clientIp}:{context.Request.Path}";

            var rateLimitInfo = _rateLimitStore.GetOrAdd(key, _ => new RateLimitInfo
            {
                RequestCount = 0,
                WindowStart = DateTime.UtcNow
            });

            // Reset window if expired
            if (DateTime.UtcNow - rateLimitInfo.WindowStart > _timeWindow)
            {
                rateLimitInfo.RequestCount = 0;
                rateLimitInfo.WindowStart = DateTime.UtcNow;
            }

            // Check rate limit
            if (rateLimitInfo.RequestCount >= _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Path}", clientIp, context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    $"{{\"error\":\"Rate limit exceeded. Please try again later.\",\"retryAfter\":{_timeWindow.TotalSeconds}}}");
                return;
            }

            // Increment request count
            Interlocked.Increment(ref rateLimitInfo.RequestCount);

            // Add rate limit headers
            context.Response.Headers.Append("X-RateLimit-Limit", _maxRequests.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", 
                Math.Max(0, _maxRequests - rateLimitInfo.RequestCount).ToString());
            context.Response.Headers.Append("X-RateLimit-Reset", 
                (rateLimitInfo.WindowStart.Add(_timeWindow) - DateTime.UtcNow).TotalSeconds.ToString());

            await _next(context);
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP (when behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private class RateLimitInfo
        {
            public int RequestCount;
            public DateTime WindowStart;
        }
    }
}

