using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Factory for creating HttpTool instances that connect to local CalqCmdController endpoints
/// Automatically discovers the controller's URL from ASP.NET Core configuration
/// </summary>
public class LocalHttpToolFactory
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly string? _explicitBaseUrl;
    private readonly HttpClient? _sharedHttpClient;

    /// <summary>
    /// Creates a factory that automatically discovers CalqCmdController URL from ASP.NET Core
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
    /// <param name="httpClient">Optional shared HttpClient</param>
    public LocalHttpToolFactory(string baseUrl, HttpClient? httpClient = null)
    {
        _explicitBaseUrl = baseUrl.TrimEnd('/');
        _sharedHttpClient = httpClient;
    }

    /// <summary>
    /// Creates an HttpTool instance configured to connect to the CalqCmdController
    /// </summary>
    /// <returns>HttpTool configured with the appropriate base URL</returns>
    public HttpTool CreateHttpTool()
    {
        var httpClient = _sharedHttpClient ?? new HttpClient();
        var baseUrl = GetBaseUrl();
        httpClient.BaseAddress = new Uri(baseUrl + "/");
        return new HttpTool(httpClient);
    }

    /// <summary>
    /// Creates an HttpTool instance with a custom HttpClient configuration
    /// </summary>
    /// <param name="configureClient">Action to configure the HttpClient before creating the HttpTool</param>
    /// <returns>HttpTool configured with the custom HttpClient</returns>
    public HttpTool CreateHttpTool(Action<HttpClient> configureClient)
    {
        var httpClient = _sharedHttpClient ?? new HttpClient();
        var baseUrl = GetBaseUrl();
        httpClient.BaseAddress = new Uri(baseUrl + "/");
        configureClient(httpClient);
        return new HttpTool(httpClient);
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

        // Get the server addresses
        var server = _serviceProvider.GetService<IServer>();
        var addresses = server?.Features.Get<IServerAddressesFeature>()?.Addresses;
        
        string hostUrl;
        if (addresses?.Any() == true)
        {
            // Use the first available address
            hostUrl = addresses.First().TrimEnd('/');
        }
        else
        {
            // Fallback to localhost with HTTPS
            hostUrl = "https://localhost:5001";
        }

        // Get the route prefix for CalqCmdController from options
        var routePrefix = GetCalqCmdControllerRoutePrefix();
        
        return string.IsNullOrEmpty(routePrefix) 
            ? $"{hostUrl}/CalqCmd" 
            : $"{hostUrl}/{routePrefix}";
    }

    private string? GetCalqCmdControllerRoutePrefix()
    {
        if (_serviceProvider == null)
        {
            return null;
        }

        try
        {
            // Get the route prefix from the registered options
            var options = _serviceProvider.GetService<IOptions<CalqCmdControllerOptions>>();
            return options?.Value?.RoutePrefix;
        }
        catch
        {
            // If we can't get the options, fall back to default
            return null;
        }
    }
}