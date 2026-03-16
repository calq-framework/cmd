using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Factory for creating HttpTool instances that connect to local CalqCmdController endpoints
///     Automatically discovers the controller's URL from ASP.NET Core configuration
/// </summary>
public class LocalHttpToolFactory : ILocalToolFactory, IDisposable {
    private readonly string? _explicitBaseUrl;
    private readonly IServiceProvider? _serviceProvider;
    private readonly HttpClient? _sharedHttpClient;
    private bool _disposed;

    /// <summary>
    ///     Creates a factory that automatically discovers CalqCmdController URL from ASP.NET Core
    ///     Uses IHttpClientFactory for optimal HttpClient management
    /// </summary>
    /// <param name="serviceProvider">Service provider to access ASP.NET Core services</param>
    public LocalHttpToolFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <summary>
    ///     Creates a factory with explicit base URL (for backwards compatibility)
    /// </summary>
    /// <param name="baseUrl">Base URL where CalqCmdController is hosted</param>
    /// <param name="httpClient">Optional shared HttpClient - if provided, caller is responsible for disposal</param>
    public LocalHttpToolFactory(string baseUrl, HttpClient? httpClient = null) {
        _explicitBaseUrl = baseUrl.TrimEnd('/');
        _sharedHttpClient = httpClient;
    }

    public IShell CreateLocalTool() {
        ThrowIfDisposed();

        HttpClient httpClient = GetOrCreateHttpClient();
        string baseUrl = GetBaseUrl();
        httpClient.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/");
        return new HttpTool(httpClient);
    }

    /// <summary>
    ///     Disposes the factory.
    ///     Note: HttpClient instances created by IHttpClientFactory are managed by the factory.
    ///     Only HttpClient instances created directly (fallback scenario) need disposal consideration.
    /// </summary>
    public void Dispose() {
        if (!_disposed) {
            // IHttpClientFactory manages HttpClient lifecycle - no manual disposal needed
            // Provided HttpClient instances are owned by the caller
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    ///     Creates an HttpTool instance with a custom HttpClient configuration
    /// </summary>
    /// <param name="configureClient">Action to configure the HttpClient before creating the HttpTool</param>
    /// <returns>HttpTool configured with the custom HttpClient</returns>
    public HttpTool CreateHttpTool(Action<HttpClient> configureClient) {
        ThrowIfDisposed();

        HttpClient httpClient = GetOrCreateHttpClient();
        string baseUrl = GetBaseUrl();
        httpClient.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/");
        configureClient(httpClient);
        return new HttpTool(httpClient);
    }

    private HttpClient GetOrCreateHttpClient() {
        if (_sharedHttpClient != null) {
            return _sharedHttpClient;
        }

        if (_serviceProvider != null) {
            IHttpClientFactory? httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            if (httpClientFactory != null) {
                string httpClientName = GetHttpClientName();
                return httpClientFactory.CreateClient(httpClientName);
            }

            throw new InvalidOperationException("IHttpClientFactory is not registered. Ensure AddLocalToolFactory() is called to register required services.");
        }

        return new HttpClient();
    }

    private string GetBaseUrl() {
        if (!string.IsNullOrEmpty(_explicitBaseUrl)) {
            return _explicitBaseUrl;
        }

        if (_serviceProvider == null) {
            throw new InvalidOperationException("ServiceProvider is required for automatic URL discovery");
        }

        IServer? server = _serviceProvider.GetService<IServer>();
        ICollection<string>? addresses = server?.Features.Get<IServerAddressesFeature>()
            ?.Addresses;

        string hostUrl;
        if (addresses?.Count > 0) {
            hostUrl = addresses.First()
                .TrimEnd('/');
        } else {
            IHttpContextAccessor? httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            HttpContext? httpContext = httpContextAccessor?.HttpContext;

            if (httpContext != null) {
                HttpRequest? request = httpContext.Request;
                string? scheme = request.Scheme;
                string? host = request.Host.Value;
                hostUrl = $"{scheme}://{host}";
            } else {
                hostUrl = "https://localhost:5001";
            }
        }

        string? routePrefix = GetCalqCmdControllerRoutePrefix();
        string normalizedPrefix = NormalizeRoutePrefix(routePrefix);

        return $"{hostUrl}/{normalizedPrefix}";
    }

    private string NormalizeRoutePrefix(string? prefix) =>
        string.IsNullOrEmpty(prefix)
            ? nameof(CalqCmdController)
                .Replace("Controller", "")
            : prefix.Trim('/');

    private string? GetCalqCmdControllerRoutePrefix() {
        if (_serviceProvider == null) {
            return null;
        }

        try {
            IOptions<CalqCmdControllerOptions>? options = _serviceProvider.GetService<IOptions<CalqCmdControllerOptions>>();
            return options?.Value?.RoutePrefix;
        } catch {
            return null;
        }
    }

    private string GetHttpClientName() {
        if (_serviceProvider == null) {
            return "CalqFramework.Cmd.LocalHttpTool";
        }

        try {
            IOptions<CalqCmdControllerOptions>? options = _serviceProvider.GetService<IOptions<CalqCmdControllerOptions>>();
            return options?.Value?.HttpClientName ?? "CalqFramework.Cmd.LocalHttpTool";
        } catch {
            return "CalqFramework.Cmd.LocalHttpTool";
        }
    }

    private void ThrowIfDisposed() {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(LocalHttpToolFactory));
        }
    }
}
