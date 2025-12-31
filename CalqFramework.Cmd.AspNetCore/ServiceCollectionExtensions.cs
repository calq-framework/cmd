using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.Extensions.DependencyInjection;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Extension methods for registering CalqFramework.Cmd services with ASP.NET Core dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
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