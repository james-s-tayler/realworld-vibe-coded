using System.Net.Http;

namespace Server.Web.Infrastructure;

public class SpaDevServerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _devServerUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpaDevServerMiddleware> _logger;

    public SpaDevServerMiddleware(RequestDelegate next, string devServerUrl, ILogger<SpaDevServerMiddleware> logger)
    {
        _next = next;
        _devServerUrl = devServerUrl.TrimEnd('/');
        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only proxy requests that are not API requests
        if (context.Request.Path.StartsWithSegments("/api") || 
            context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Check if this is a file request (has extension)
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path) && Path.HasExtension(path))
        {
            // This is a file request - proxy it to Vite dev server
            await ProxyToDevServer(context);
            return;
        }

        // For HTML/route requests, proxy to dev server as well
        // Vite dev server will handle client-side routing
        await ProxyToDevServer(context);
    }

    private async Task ProxyToDevServer(HttpContext context)
    {
        try
        {
            var requestUri = $"{_devServerUrl}{context.Request.Path}{context.Request.QueryString}";
            var method = new HttpMethod(context.Request.Method);
            var requestMessage = new HttpRequestMessage(method, requestUri);

            // Copy headers (skip Host header as it will be set automatically)
            foreach (var header in context.Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // Copy request body for POST/PUT requests
            if (context.Request.ContentLength > 0)
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
                if (context.Request.ContentType != null)
                    requestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", context.Request.ContentType);
            }

            var response = await _httpClient.SendAsync(requestMessage);

            // Copy response status
            context.Response.StatusCode = (int)response.StatusCode;

            // Copy response headers
            foreach (var header in response.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in response.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // Copy response body
            await response.Content.CopyToAsync(context.Response.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy request to Vite dev server: {DevServerUrl}", _devServerUrl);
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("Vite dev server not available. Make sure it's running on " + _devServerUrl);
        }
    }
}

public static class SpaDevServerMiddlewareExtensions
{
    public static IApplicationBuilder UseSpaDevServer(this IApplicationBuilder builder, string devServerUrl)
    {
        return builder.UseMiddleware<SpaDevServerMiddleware>(devServerUrl);
    }
}