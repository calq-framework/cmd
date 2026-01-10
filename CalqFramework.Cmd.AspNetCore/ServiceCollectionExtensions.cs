using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Configuration options for CalqCmdController routing
/// </summary>
public class CalqCmdControllerOptions
{
    /// <summary>
    /// Route prefix for the CalqCmdController. If null or empty, uses default "CalqCmd"
    /// </summary>
    public string? RoutePrefix { get; set; }
}

/// <summary>
/// Extension methods for registering CalqFramework.Cmd services with ASP.NET Core dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with CalqFramework.Cli integration.
    /// This controller provides streaming endpoints using CalqFramework.Cli for command execution.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTarget">The target object to pass to CalqFramework.Cli for command execution.</param>
    /// <param name="routePrefix">Optional route prefix for the controller. If provided, controller will be available at /{routePrefix}/ instead of /CalqCmd/</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, object cliTarget, string? routePrefix = null)
    {
        return AddCalqCmdController(services, cliTarget, routePrefix, null);
    }

    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with CalqFramework.Cli integration.
    /// This controller provides streaming endpoints using CalqFramework.Cli for command execution.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTarget">The target object to pass to CalqFramework.Cli for command execution.</param>
    /// <param name="routePrefix">Optional route prefix for the controller. If provided, controller will be available at /{routePrefix}/ instead of /CalqCmd/</param>
    /// <param name="configureCacheOptions">Optional action to configure cache options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, object cliTarget, string? routePrefix, Action<CalqCmdCacheOptions>? configureCacheOptions)
    {
        services.AddSingleton(cliTarget);
        
        // Register cache options
        if (configureCacheOptions != null)
        {
            services.Configure(configureCacheOptions);
        }
        else
        {
            services.Configure<CalqCmdCacheOptions>(_ => { });
        }
        
        // Ensure distributed cache is available - register memory cache as fallback if none registered
        EnsureDistributedCacheRegistered(services);
        
        services.Configure<CalqCmdControllerOptions>(options =>
        {
            options.RoutePrefix = routePrefix;
        });

        if (!string.IsNullOrEmpty(routePrefix))
        {
            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.Conventions.Add(new CalqCmdControllerRouteConvention(routePrefix));
            });
        }
        
        services.AddTransient<CalqCmdController>(provider =>
        {
            var target = provider.GetRequiredService<object>();
            var localToolFactory = provider.GetRequiredService<ILocalToolFactory>();
            var distributedCache = provider.GetRequiredService<IDistributedCache>();
            var cacheOptions = provider.GetRequiredService<IOptions<CalqCmdCacheOptions>>();
            return new CalqCmdController(target, localToolFactory, distributedCache, cacheOptions);
        });
        
        return services;
    }

    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with CalqFramework.Cli integration.
    /// This overload uses a factory function to create the CLI target.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTargetFactory">Factory function to create the CLI target object.</param>
    /// <param name="routePrefix">Optional route prefix for the controller. If provided, controller will be available at /{routePrefix}/ instead of /CalqCmd/</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, Func<IServiceProvider, object> cliTargetFactory, string? routePrefix = null)
    {
        return AddCalqCmdController(services, cliTargetFactory, routePrefix, null);
    }

    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with CalqFramework.Cli integration.
    /// This overload uses a factory function to create the CLI target.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTargetFactory">Factory function to create the CLI target object.</param>
    /// <param name="routePrefix">Optional route prefix for the controller. If provided, controller will be available at /{routePrefix}/ instead of /CalqCmd/</param>
    /// <param name="configureCacheOptions">Optional action to configure cache options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, Func<IServiceProvider, object> cliTargetFactory, string? routePrefix, Action<CalqCmdCacheOptions>? configureCacheOptions)
    {
        services.AddSingleton(cliTargetFactory);
        
        // Register cache options
        if (configureCacheOptions != null)
        {
            services.Configure(configureCacheOptions);
        }
        else
        {
            services.Configure<CalqCmdCacheOptions>(_ => { });
        }
        
        // Ensure distributed cache is available - register memory cache as fallback if none registered
        EnsureDistributedCacheRegistered(services);
        
        services.Configure<CalqCmdControllerOptions>(options =>
        {
            options.RoutePrefix = routePrefix;
        });

        if (!string.IsNullOrEmpty(routePrefix))
        {
            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.Conventions.Add(new CalqCmdControllerRouteConvention(routePrefix));
            });
        }
        
        services.AddTransient<CalqCmdController>(provider =>
        {
            var factory = provider.GetRequiredService<Func<IServiceProvider, object>>();
            var target = factory(provider);
            var localToolFactory = provider.GetRequiredService<ILocalToolFactory>();
            var distributedCache = provider.GetRequiredService<IDistributedCache>();
            var cacheOptions = provider.GetRequiredService<IOptions<CalqCmdCacheOptions>>();
            return new CalqCmdController(target, localToolFactory, distributedCache, cacheOptions);
        });
        
        return services;
    }

    /// <summary>
    /// Registers a LocalHttpToolFactory that automatically discovers CalqCmdController URL from ASP.NET Core
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddLocalHttpToolFactory(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        
        services.AddHttpClient("CalqFramework.Cmd.LocalHttpTool", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        services.AddSingleton<ILocalToolFactory, LocalHttpToolFactory>();
        return services;
    }

    /// <summary>
    /// Registers a LocalHttpToolFactory with custom HttpClient configuration
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureClient">Action to configure the HttpClient</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddLocalHttpToolFactory(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        services.AddHttpContextAccessor();
        
        services.AddHttpClient("CalqFramework.Cmd.LocalHttpTool", configureClient);
        
        services.AddSingleton<ILocalToolFactory, LocalHttpToolFactory>();
        return services;
    }

    /// <summary>
    /// Registers a LocalHttpToolFactory for creating HttpTool instances that connect to CalqCmdController
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="host">Host URL where the application is running (e.g., "https://localhost:5000")</param>
    /// <param name="routePrefix">Route prefix used when registering CalqCmdController. Should match the prefix used in AddCalqCmdController</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddLocalHttpToolFactory(this IServiceCollection services, string host, string? routePrefix = null)
    {
        var baseUrl = BuildBaseUrl(host, routePrefix);
        services.AddSingleton<ILocalToolFactory>(provider => new LocalHttpToolFactory(baseUrl));
        return services;
    }

    /// <summary>
    /// Registers a LocalHttpToolFactory with a shared HttpClient for creating HttpTool instances
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="host">Host URL where the application is running (e.g., "https://localhost:5000")</param>
    /// <param name="routePrefix">Route prefix used when registering CalqCmdController. Should match the prefix used in AddCalqCmdController</param>
    /// <param name="configureHttpClient">Action to configure the shared HttpClient</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddLocalHttpToolFactory(this IServiceCollection services, string host, string? routePrefix, Action<HttpClient> configureHttpClient)
    {
        var httpClient = new HttpClient();
        configureHttpClient(httpClient);
        
        var baseUrl = BuildBaseUrl(host, routePrefix);
        services.AddSingleton<ILocalToolFactory>(provider => new LocalHttpToolFactory(baseUrl, httpClient));
        return services;
    }

    private static string BuildBaseUrl(string host, string? routePrefix)
    {
        var trimmedHost = host.TrimEnd('/');
        var normalizedPrefix = string.IsNullOrEmpty(routePrefix) ? 
            nameof(CalqCmdController).Replace("Controller", "") : 
            routePrefix.Trim('/');
        return $"{trimmedHost}/{normalizedPrefix}";
    }

    private static void EnsureDistributedCacheRegistered(IServiceCollection services)
    {
        // Check if IDistributedCache is already registered
        var distributedCacheDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDistributedCache));
        
        if (distributedCacheDescriptor == null)
        {
            // No distributed cache registered, add memory cache as default
            services.AddDistributedMemoryCache();
        }
    }

    /// <summary>
    /// Registers PythonToolServer and PythonTool services for dependency injection.
    /// PythonToolServer is registered as a singleton to manage the Python process lifecycle.
    /// PythonTool is registered as transient and depends on the PythonToolServer.
    /// Note: The PythonToolServer must be started before PythonTool can be resolved.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="scriptPath">Path to the Python script file that will be executed by the PythonToolServer.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddPythonTool("path/to/script.py");
    /// 
    /// // Start the server during application startup:
    /// var app = builder.Build();
    /// await app.Services.StartPythonToolServerAsync();
    /// 
    /// // Usage in controller:
    /// [ApiController]
    /// public class DataController : ControllerBase
    /// {
    ///     private readonly PythonTool _pythonTool;
    ///     
    ///     public DataController(PythonTool pythonTool)
    ///     {
    ///         _pythonTool = pythonTool;
    ///     }
    ///     
    ///     [HttpGet]
    ///     public async Task&lt;string&gt; ProcessData()
    ///     {
    ///         LocalTerminal.Shell = _pythonTool;
    ///         return CMD("process_data");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddPythonTool(this IServiceCollection services, string scriptPath)
    {
        services.AddSingleton<PythonToolServer>(provider => new PythonToolServer(scriptPath));
        services.AddTransient<PythonTool>(provider => 
        {
            var server = provider.GetRequiredService<PythonToolServer>();
            // Note: Server must be started before this can be resolved
            return new PythonTool(server);
        });
        return services;
    }

    /// <summary>
    /// Registers PythonToolServer and PythonTool services using a factory function.
    /// This overload allows for more complex configuration scenarios.
    /// Note: The PythonToolServer must be started before PythonTool can be resolved.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="serverFactory">Factory function to create the PythonToolServer instance.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddPythonTool(provider => 
    /// {
    ///     var config = provider.GetRequiredService&lt;IConfiguration&gt;();
    ///     var scriptPath = config["PythonScript:Path"];
    ///     return new PythonToolServer(scriptPath);
    /// });
    /// 
    /// // Start the server during application startup:
    /// var app = builder.Build();
    /// await app.Services.StartPythonToolServerAsync();
    /// </code>
    /// </example>
    public static IServiceCollection AddPythonTool(this IServiceCollection services, Func<IServiceProvider, PythonToolServer> serverFactory)
    {
        services.AddSingleton(serverFactory);
        services.AddTransient<PythonTool>(provider => 
        {
            var server = provider.GetRequiredService<PythonToolServer>();
            // Note: Server must be started before this can be resolved
            return new PythonTool(server);
        });
        return services;
    }
}

/// <summary>
/// Extension methods for IServiceProvider to manage PythonToolServer lifecycle.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Starts the registered PythonToolServer, enabling PythonTool resolution.
    /// This method should be called during application startup after the service provider is built.
    /// </summary>
    /// <param name="services">The service provider containing the registered PythonToolServer.</param>
    /// <param name="cancellationToken">Optional cancellation token for the startup operation.</param>
    /// <returns>The service provider for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if PythonToolServer is not registered.</exception>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// await app.Services.StartPythonToolServerAsync();
    /// 
    /// // Now PythonTool can be resolved from DI
    /// var pythonTool = app.Services.GetRequiredService&lt;PythonTool&gt;();
    /// </code>
    /// </example>
    public static async Task<IServiceProvider> StartPythonToolServerAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var server = services.GetRequiredService<PythonToolServer>();
        await server.StartAsync(cancellationToken);
        return services;
    }
}

/// <summary>
/// Route convention to customize CalqCmdController route prefix
/// </summary>
internal class CalqCmdControllerRouteConvention : Microsoft.AspNetCore.Mvc.ApplicationModels.IControllerModelConvention
{
    private readonly string _routePrefix;

    public CalqCmdControllerRouteConvention(string routePrefix)
    {
        _routePrefix = routePrefix.TrimStart('/').TrimEnd('/');
    }

    public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel controller)
    {
        if (controller.ControllerType == typeof(CalqCmdController))
        {
            // Remove existing route attributes
            controller.Selectors.Clear();
            
            // Add new selector with custom prefix
            var selector = new Microsoft.AspNetCore.Mvc.ApplicationModels.SelectorModel();
            selector.AttributeRouteModel = new Microsoft.AspNetCore.Mvc.ApplicationModels.AttributeRouteModel
            {
                Template = _routePrefix
            };
            controller.Selectors.Add(selector);
        }
    }
}