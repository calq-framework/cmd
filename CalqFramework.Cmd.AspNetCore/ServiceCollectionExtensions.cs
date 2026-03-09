using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Extension methods for registering CalqFramework.Cmd services with ASP.NET Core dependency injection.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    ///     Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    ///     This controller provides streaming endpoints for command execution.
    ///     Uses CalqCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    ///     Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    ///     Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="commandTarget">The target object containing methods to execute. This will be injected into the command executor.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, object commandTarget,
        Action<CalqCmdControllerOptions>? configure = null) => services.AddCalqCmdController(_ => commandTarget, configure);

    /// <summary>
    ///     Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    ///     This controller provides streaming endpoints for command execution.
    ///     Uses CalqCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    ///     Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    ///     Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="commandTarget">The target object containing methods to execute. This will be injected into the command executor.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <param name="configureCacheOptions">Optional action to configure cache options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services, object commandTarget,
        Action<CalqCmdControllerOptions>? configure, Action<CalqCmdCacheOptions>? configureCacheOptions) =>
        services.AddCalqCmdController(_ => commandTarget, configure, configureCacheOptions);

    /// <summary>
    ///     Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    ///     This controller provides streaming endpoints for command execution.
    ///     Uses CalqCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    ///     Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    ///     Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="commandTargetFactory">Factory function to create the command target object containing methods to execute. This will be injected into the command executor.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services,
        Func<IServiceProvider, object> commandTargetFactory, Action<CalqCmdControllerOptions>? configure = null) =>
        AddCalqCmdControllerInternal(services, commandTargetFactory, configure, null);

    /// <summary>
    ///     Registers the CalqCmdController for ASP.NET Core MVC with command execution support.
    ///     This controller provides streaming endpoints for command execution.
    ///     Uses CalqCommandExecutor (CalqFramework.Cli) by default, but can be customized via options.
    ///     Automatically registers DistributedMemoryCache if no distributed cache is already registered.
    ///     Also registers LocalHttpToolFactory for creating HTTP tools that connect to the controller.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="commandTargetFactory">Factory function to create the command target object containing methods to execute. This will be injected into the command executor.</param>
    /// <param name="configure">Optional action to configure CalqCmdController options.</param>
    /// <param name="configureCacheOptions">Optional action to configure cache options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCalqCmdController(this IServiceCollection services,
        Func<IServiceProvider, object> commandTargetFactory, Action<CalqCmdControllerOptions>? configure,
        Action<CalqCmdCacheOptions>? configureCacheOptions) =>
        AddCalqCmdControllerInternal(services, commandTargetFactory, configure, configureCacheOptions);

    /// <summary>
    ///     Internal implementation for registering CalqCmdController with shared logic.
    /// </summary>
    private static IServiceCollection AddCalqCmdControllerInternal(IServiceCollection services,
        Func<IServiceProvider, object> commandTargetFactory, Action<CalqCmdControllerOptions>? configure,
        Action<CalqCmdCacheOptions>? configureCacheOptions) {
        // Configure controller options
        CalqCmdControllerOptions options = new();
        configure?.Invoke(options);

        services.Configure<CalqCmdControllerOptions>(opts => {
            opts.RoutePrefix = options.RoutePrefix;
            opts.HttpClientTimeout = options.HttpClientTimeout;
            opts.HttpClientName = options.HttpClientName;
            opts.CommandExecutor = options.CommandExecutor;
            opts.DefaultShell = options.DefaultShell;
            opts.DefaultTerminalLogger = options.DefaultTerminalLogger;
        });

        // Register command executor - use custom if provided, otherwise default to CalqCommandExecutor
        if (options.CommandExecutor != null) {
            services.AddSingleton(options.CommandExecutor);
        } else {
            services.AddSingleton<ICalqCommandExecutor>(provider => {
                object target = commandTargetFactory(provider);
                return new CalqCommandExecutor(target);
            });
        }

        // Register cache options
        if (configureCacheOptions != null) {
            services.Configure(configureCacheOptions);
        } else {
            services.Configure<CalqCmdCacheOptions>(_ => { });
        }

        // Ensure distributed cache is available - register memory cache as fallback if none registered
        EnsureDistributedCacheRegistered(services);

        // Register LocalHttpToolFactory that automatically discovers CalqCmdController URL
        services.AddLocalToolFactoryInternal(options);

        // Register global filter for automatic LocalTerminal configuration
        services.Configure<MvcOptions>(mvcOptions => {
            mvcOptions.Filters.Add<LocalTerminalFilter>();
            
            // Configure routing if custom prefix is specified
            if (!string.IsNullOrEmpty(options.RoutePrefix)) {
                mvcOptions.Conventions.Add(new CalqCmdControllerRouteConvention(options.RoutePrefix));
            }
        });

        services.AddTransient<CalqCmdController>(provider => {
            ICalqCommandExecutor calqCommandExecutor = provider.GetRequiredService<ICalqCommandExecutor>();
            ILocalToolFactory localToolFactory = provider.GetRequiredService<ILocalToolFactory>();
            IDistributedCache distributedCache = provider.GetRequiredService<IDistributedCache>();
            IOptions<CalqCmdCacheOptions> cacheOptions = provider.GetRequiredService<IOptions<CalqCmdCacheOptions>>();
            return new CalqCmdController(calqCommandExecutor, localToolFactory, distributedCache, cacheOptions);
        });

        return services;
    }

    /// <summary>
    ///     Registers a LocalHttpToolFactory that automatically discovers CalqCmdController URL from ASP.NET Core
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="options">Controller options containing HTTP client configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    private static IServiceCollection AddLocalToolFactoryInternal(this IServiceCollection services,
        CalqCmdControllerOptions options) {
        services.AddHttpContextAccessor();

        services.AddHttpClient(options.HttpClientName, client => { client.Timeout = options.HttpClientTimeout; });

        services.AddSingleton<ILocalToolFactory, LocalHttpToolFactory>();
        return services;
    }

    private static void EnsureDistributedCacheRegistered(IServiceCollection services) {
        // Check if IDistributedCache is already registered
        ServiceDescriptor? distributedCacheDescriptor =
            services.FirstOrDefault(d => d.ServiceType == typeof(IDistributedCache));

        if (distributedCacheDescriptor == null) {
            // No distributed cache registered, add memory cache as default
            services.AddDistributedMemoryCache();
        }
    }

    /// <summary>
    ///     Registers PythonToolServer and PythonTool services for dependency injection.
    ///     PythonToolServer is registered as a singleton to manage the Python process lifecycle.
    ///     PythonTool is registered as transient and depends on the PythonToolServer.
    ///     Note: The PythonToolServer must be started before PythonTool can be resolved.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="scriptPath">Path to the Python script file that will be executed by the PythonToolServer.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    ///     <code>
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
    public static IServiceCollection AddPythonTool(this IServiceCollection services, string scriptPath) {
        services.AddSingleton<PythonToolServer>(provider => new PythonToolServer(scriptPath));
        services.AddTransient<PythonTool>(provider => {
            PythonToolServer server = provider.GetRequiredService<PythonToolServer>();
            // Note: Server must be started before this can be resolved
            return new PythonTool(server);
        });
        return services;
    }

    /// <summary>
    ///     Registers PythonToolServer and PythonTool services using a factory function.
    ///     This overload allows for more complex configuration scenarios.
    ///     Note: The PythonToolServer must be started before PythonTool can be resolved.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="serverFactory">Factory function to create the PythonToolServer instance.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    ///     <code>
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
    public static IServiceCollection AddPythonTool(this IServiceCollection services,
        Func<IServiceProvider, PythonToolServer> serverFactory) {
        services.AddSingleton(serverFactory);
        services.AddTransient<PythonTool>(provider => {
            PythonToolServer server = provider.GetRequiredService<PythonToolServer>();
            // Note: Server must be started before this can be resolved
            return new PythonTool(server);
        });
        return services;
    }
}

/// <summary>
///     Extension methods for IServiceProvider to manage PythonToolServer lifecycle.
/// </summary>
public static class ServiceProviderExtensions {
    /// <summary>
    ///     Starts the registered PythonToolServer, enabling PythonTool resolution.
    ///     This method should be called during application startup after the service provider is built.
    /// </summary>
    /// <param name="services">The service provider containing the registered PythonToolServer.</param>
    /// <param name="cancellationToken">Optional cancellation token for the startup operation.</param>
    /// <returns>The service provider for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if PythonToolServer is not registered.</exception>
    /// <example>
    ///     <code>
    /// var app = builder.Build();
    /// await app.Services.StartPythonToolServerAsync();
    /// 
    /// // Now PythonTool can be resolved from DI
    /// var pythonTool = app.Services.GetRequiredService&lt;PythonTool&gt;();
    /// </code>
    /// </example>
    public static async Task<IServiceProvider> StartPythonToolServerAsync(this IServiceProvider services,
        CancellationToken cancellationToken = default) {
        PythonToolServer server = services.GetRequiredService<PythonToolServer>();
        await server.StartAsync(cancellationToken);
        return services;
    }
}

/// <summary>
///     Route convention to customize CalqCmdController route prefix
/// </summary>
internal class CalqCmdControllerRouteConvention(string routePrefix) : IControllerModelConvention {
    private readonly string _routePrefix = routePrefix.TrimStart('/').TrimEnd('/');

    public void Apply(ControllerModel controller) {
        if (controller.ControllerType == typeof(CalqCmdController)) {
            // Remove existing route attributes
            controller.Selectors.Clear();

            // Add new selector with custom prefix
            SelectorModel selector = new() {
                AttributeRouteModel = new AttributeRouteModel {
                    Template = _routePrefix
                }
            };
            controller.Selectors.Add(selector);
        }
    }
}
