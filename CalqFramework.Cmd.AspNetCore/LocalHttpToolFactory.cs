using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Factory for creating HttpTool instances that connect to local CalqCmdController endpoints
/// Automatically discovers the controller's URL from ASP.NET Core configuration
/// </summary>
public class LocalHttpToolFactory : ILocalToolFactory, IDisposable
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly string? _explicitBaseUrl;
    private readonly HttpClient? _sharedHttpClient;
    private bool _disposed;

    /// <summary>
    /// Creates a factory that automatically discovers CalqCmdController URL from ASP.NET Core
    /// Uses IHttpClientFactory for optimal HttpClient management
    /// </summary>
    /// <param name="serviceProvider">Service provider to access ASP.NET Core services</param>
    public LocalHttpToolFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a factory with explicit base URL (for backwards compatibility)
    /// </summary>
    /// <param name="baseUrl">Base URL where CalqCmdController is hosted</param>
    /// <param name="httpClient">Optional shared HttpClient - if provided, caller is responsible for disposal</param>
    public LocalHttpToolFactory(string baseUrl, HttpClient? httpClient = null)
    {
        _explicitBaseUrl = baseUrl.TrimEnd('/');
        _sharedHttpClient = httpClient;
    }

    public IShell CreateLocalTool()
    {
        ThrowIfDisposed();
        
        var httpClient = GetOrCreateHttpClient();
        var baseUrl = GetBaseUrl();
        httpClient.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/");
        return new HttpTool(httpClient);
    }

    /// <summary>
    /// Creates an HttpTool instance with a custom HttpClient configuration
    /// </summary>
    /// <param name="configureClient">Action to configure the HttpClient before creating the HttpTool</param>
    /// <returns>HttpTool configured with the custom HttpClient</returns>
    public HttpTool CreateHttpTool(Action<HttpClient> configureClient)
    {
        ThrowIfDisposed();
        
        var httpClient = GetOrCreateHttpClient();
        var baseUrl = GetBaseUrl();
        httpClient.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/");
        configureClient(httpClient);
        return new HttpTool(httpClient);
    }

    private HttpClient GetOrCreateHttpClient()
    {
        if (_sharedHttpClient != null)
        {
            return _sharedHttpClient;
        }

        if (_serviceProvider != null)
        {
            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            if (httpClientFactory != null)
            {
                return httpClientFactory.CreateClient("CalqFramework.Cmd.LocalHttpTool");
            }
            
            throw new InvalidOperationException(
                "IHttpClientFactory is not registered. Ensure AddLocalHttpToolFactory() is called to register required services.");
        }

        return new HttpClient();
    }

    private string GetBaseUrl()
    {
        if (!string.IsNullOrEmpty(_explicitBaseUrl))
        {
            return _explicitBaseUrl;
        }

        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("ServiceProvider is required for automatic URL discovery");
        }

        var server = _serviceProvider.GetService<IServer>();
        var addresses = server?.Features.Get<IServerAddressesFeature>()?.Addresses;
        
        string hostUrl;
        if (addresses?.Any() == true)
        {
            hostUrl = addresses.First().TrimEnd('/');
        }
        else
        {
            var httpContextAccessor = _serviceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var httpContext = httpContextAccessor?.HttpContext;
            
            if (httpContext != null)
            {
                var request = httpContext.Request;
                var scheme = request.Scheme;
                var host = request.Host.Value;
                hostUrl = $"{scheme}://{host}";
            }
            else
            {
                hostUrl = "https://localhost:5001";
            }
        }

        var routePrefix = GetCalqCmdControllerRoutePrefix();
        var normalizedPrefix = NormalizeRoutePrefix(routePrefix);
        
        return $"{hostUrl}/{normalizedPrefix}";
    }

    private string NormalizeRoutePrefix(string? prefix)
    {
        return string.IsNullOrEmpty(prefix) ? 
            nameof(CalqCmdController).Replace("Controller", "") : 
            prefix.Trim('/');
    }

    private string? GetCalqCmdControllerRoutePrefix()
    {
        if (_serviceProvider == null)
        {
            return null;
        }

        try
        {
            var options = _serviceProvider.GetService<IOptions<CalqCmdControllerOptions>>();
            return options?.Value?.RoutePrefix;
        }
        catch
        {
            return null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(LocalHttpToolFactory));
        }
    }

    /// <summary>
    /// Disposes the factory. 
    /// Note: HttpClient instances created by IHttpClientFactory are managed by the factory.
    /// Only HttpClient instances created directly (fallback scenario) need disposal consideration.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // IHttpClientFactory manages HttpClient lifecycle - no manual disposal needed
            // Provided HttpClient instances are owned by the caller
            _disposed = true;
        }
    }
}