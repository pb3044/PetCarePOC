using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PetCarePlatform.Web.Middleware
{
    /// <summary>
    /// Middleware to add security headers to HTTP responses.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public SecurityHeadersMiddleware(
            RequestDelegate next,
            ILogger<SecurityHeadersMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Build Content Security Policy
            // All resources are served locally - no CDN dependencies for better security and performance
            var connectSrc = "'self'";
            
            // In development, allow WebSocket and HTTP connections for browser refresh and BrowserLink
            // Note: CSP doesn't support port wildcards (*), so BrowserLink may still be blocked.
            // If BrowserLink errors persist, disable it in launchSettings.json by setting "enableBrowserLink": false
            if (_environment.IsDevelopment())
            {
                // Allow localhost connections with any port for browser refresh and BrowserLink
                connectSrc += " http://localhost:* ws://localhost:* http://127.0.0.1:* ws://127.0.0.1:*";
            }
            
            // If you need to use CDNs (e.g., Leaflet for maps), uncomment the CDN domains below
            // For production with all resources localized, use the strict CSP below
            var scriptSrc = "'self' 'unsafe-inline' 'unsafe-eval'";
            var styleSrc = "'self' 'unsafe-inline'";
            var fontSrc = "'self' data:";

            // CDN support removed - all resources are now local
            
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src " + scriptSrc + "; " +
                "style-src " + styleSrc + "; " +
                "font-src " + fontSrc + "; " +
                "img-src 'self' data: https: blob:; " +
                "connect-src " + connectSrc + ";");

            await _next(context);
        }
    }
}

