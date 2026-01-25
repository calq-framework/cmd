using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Configuration options for CalqCmdController
/// </summary>
public class CalqCmdControllerOptions
{
    /// <summary>
    /// Route prefix for the CalqCmdController. If null or empty, uses default "CalqCmd"
    /// </summary>
    public string? RoutePrefix { get; set; }
    
    /// <summary>
    /// HTTP client timeout for LocalHttpTool connections. Default is 30 seconds.
    /// </summary>
    public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Named HTTP client configuration for CalqFramework.Cmd.LocalHttpTool
    /// </summary>
    public string HttpClientName { get; set; } = "CalqFramework.Cmd.LocalHttpTool";
    
    /// <summary>
    /// Custom command executor. If null, uses CliCommandExecutor (CalqFramework.Cli) by default.
    /// </summary>
    public ICalqCommandExecutor? CommandExecutor { get; set; }
}

/// <summary>
/// Extension methods for registering CalqFramework.Cmd services with ASP.NET Core dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    /// This controller provides streaming endpoints for command execution.
    /// Uses CliCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTarget">The target object to pass to the command executor.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, object cliTarget, Action<CalqCmdControllerOptions>? configure = null)
    {
        return AddCalqCmdController(services, _ => cliTarget, configure);
    }

    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    /// This controller provides streaming endpoints for command execution.
    /// Uses CliCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTarget">The target object to pass to the command executor.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <param name="configureCacheOptions">Optional action to configure cache options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, object cliTarget, Action<CalqCmdControllerOptions>? configure, Action<CalqCmdCacheOptions>? configureCacheOptions)
    {
        return AddCalqCmdController(services, _ => cliTarget, configure, configureCacheOptions);
    }

    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    /// This controller provides streaming endpoints for command execution.
    /// Uses CliCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTargetFactory">Factory function to create the CLI target object.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, Func<IServiceProvider, object> cliTargetFactory, Action<CalqCmdControllerOptions>? configure = null)
    {
        return AddCalqCmdControllerInternal(services, cliTargetFactory, configure, null);
    }

    /// <summary>
    /// Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    /// This controller provides streaming endpoints for command execution.
    /// Uses CliCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    /// Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    /// Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cliTargetFactory">Factory function to create the CLI target object.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <param name="configureCacheOptions">Optional action to configure cache options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, Func<IServiceProvider, object> cliTargetFactory, Action<CalqCmdControllerOptions>? configure, Action<CalqCmdCacheOptions>? configureCacheOptions)
    {
        return AddCalqCmdControllerInternal(services, cliTargetFactory, configure, configureCacheOptions);
    }

    /// <summary>
    /// Internal implementation for registering CalqCmdController with shared logic.
    /// </summary>
    private static IServiceCollection AddCalqCmdControllerInternal(IServiceCollection services, Func<IServiceProvider, object> cliTargetFactory, Action<CalqCmdControllerOptions>? configure, Action<CalqCmdCacheOptions>? configureCacheOptions)
    {
        services.AddSingleton(cliTargetFactory);
        
        // Configure controller options
        var options = new CalqCmdControllerOptions();
        configure?.Invoke(options);
        
        services.Configure<CalqCmdControllerOptions>(opts =>
        {
            opts.RoutePrefix = options.RoutePrefix;
            opts.HttpClientTimeout = options.HttpClientTimeout;
            opts.HttpClientName = options.HttpClientName;
            opts.CommandExecutor = options.CommandExecutor;
        });
        
        // Register command executor - use custom if provided, otherwise default to CliCommandExecutor
        if (options.CommandExecutor != null)
        {
            services.AddSingleton(options.CommandExecutor);
        }
        else
        {
            services.AddSingleton<ICalqCommandExecutor, CliCommandExecutor>();
        }
        
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
        
        // Register LocalHttpToolFactory that automatically discovers CalqCmdController URL
        AddLocalToolFactoryInternal(services, options);
        
        // Configure routing if custom prefix is specified
        if (!string.IsNullOrEmpty(options.RoutePrefix))
        {
            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(mvcOptions =>
            {
                mvcOptions.Conventions.Add(new CalqCmdControllerRouteConvention(options.RoutePrefix));
            });
        }
        
        services.AddTransient<CalqCmdController>(provider =>
        {
            var factory = provider.GetRequiredService<Func<IServiceProvider, object>>();
            var target = factory(provider);
            var commandExecutor = provider.GetRequiredService<ICalqCommandExecutor>();
            var localToolFactory = provider.GetRequiredService<ILocalToolFactory>();
            var distributedCache = provider.GetRequiredService<IDistributedCache>();
            var cacheOptions = provider.GetRequiredService<IOptions<CalqCmdCacheOptions>>();
            return new CalqCmdController(target, commandExecutor, localToolFactory, distributedCache, cacheOptions);
        });
        
        return services;
    }

    /// <summary>
    /// Registers a LocalHttpToolFactory that automatically discovers CalqCmdController URL from ASP.NET Core
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="options">Controller options containing HTTP client configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    private static IServiceCollection AddLocalToolFactoryInternal(this IServiceCollection services, CalqCmdControllerOptions options)
    {
        services.AddHttpContextAccessor();
        
        services.AddHttpClient(options.HttpClientName, client =>
        {
            client.Timeout = options.HttpClientTimeout;
        });
        
        services.AddSingleton<ILocalToolFactory, LocalHttpToolFactory>();
        return services;
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