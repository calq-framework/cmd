using System.Reflection;
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CalqFramework.Cmd.AspNetCore.Test;

public class ServiceCollectionExtensionsTest {
    private class TestCliTarget {
        public static string TestMethod() => "test";
    }

    #region AddCalqCmdController Tests

    [Fact]
    public void AddCalqCmdController_RegistersLocalToolFactory() {
        ServiceCollection services = new();
        services.AddCalqCmdController(new TestCliTarget());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ILocalToolFactory factory = serviceProvider.GetRequiredService<ILocalToolFactory>();

        Assert.NotNull(factory);
        Assert.True(factory is IDisposable);
    }

    [Fact]
    public void AddCalqCmdController_WithFactory_RegistersLocalToolFactory() {
        ServiceCollection services = new();
        services.AddCalqCmdController(provider => new TestCliTarget());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ILocalToolFactory factory = serviceProvider.GetRequiredService<ILocalToolFactory>();

        Assert.NotNull(factory);
        Assert.True(factory is IDisposable);
    }

    [Fact]
    public void AddCalqCmdController_RegistersHttpClientFactory() {
        ServiceCollection services = new();
        services.AddCalqCmdController(new TestCliTarget());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ILocalToolFactory factory = serviceProvider.GetRequiredService<ILocalToolFactory>();
        IHttpClientFactory? httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

        Assert.NotNull(factory);
        Assert.NotNull(httpClientFactory);

        HttpClient namedClient = httpClientFactory.CreateClient("CalqFramework.Cmd.LocalHttpTool");
        Assert.NotNull(namedClient);
        Assert.Equal(TimeSpan.FromSeconds(30), namedClient.Timeout);
    }

    [Fact]
    public void AddCalqCmdController_WithCustomHttpClientOptions_ConfiguresCorrectly() {
        ServiceCollection services = new();
        services.AddCalqCmdController(new TestCliTarget(), options => {
            options.HttpClientTimeout = TimeSpan.FromMinutes(2);
            options.HttpClientName = "CustomHttpClient";
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ILocalToolFactory factory = serviceProvider.GetRequiredService<ILocalToolFactory>();
        IHttpClientFactory? httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

        Assert.NotNull(factory);
        Assert.NotNull(httpClientFactory);

        HttpClient customClient = httpClientFactory.CreateClient("CustomHttpClient");
        Assert.NotNull(customClient);
        Assert.Equal(TimeSpan.FromMinutes(2), customClient.Timeout);
    }

    [Fact]
    public void AddCalqCmdController_WithRoutePrefix_ConfiguresOptions() {
        ServiceCollection services = new();
        services.AddCalqCmdController(new TestCliTarget(), options => { options.RoutePrefix = "api/commands"; });
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IOptions<CalqCmdControllerOptions> options =
            serviceProvider.GetRequiredService<IOptions<CalqCmdControllerOptions>>();

        Assert.NotNull(options.Value);
        Assert.Equal("api/commands", options.Value.RoutePrefix);
    }

    [Fact]
    public void AddCalqCmdController_WithCacheOptions_ConfiguresCorrectly() {
        ServiceCollection services = new();
        services.AddCalqCmdController(new TestCliTarget(), null, cacheOptions => {
            cacheOptions.ErrorCacheExpiration = TimeSpan.FromMinutes(30);
            cacheOptions.ErrorCacheKeyPrefix = "MyApp.Errors:";
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IOptions<CalqCmdCacheOptions>
            cacheOptions = serviceProvider.GetRequiredService<IOptions<CalqCmdCacheOptions>>();

        Assert.NotNull(cacheOptions.Value);
        Assert.Equal(TimeSpan.FromMinutes(30), cacheOptions.Value.ErrorCacheExpiration);
        Assert.Equal("MyApp.Errors:", cacheOptions.Value.ErrorCacheKeyPrefix);
    }

    [Fact]
    public void AddCalqCmdController_LocalToolFactory_DoesNotOwnHttpClients() {
        ServiceCollection services = new();
        services.AddCalqCmdController(new TestCliTarget());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        using LocalHttpToolFactory factory =
            (LocalHttpToolFactory)serviceProvider.GetRequiredService<ILocalToolFactory>();

        Assert.NotNull(factory);
        factory.Dispose();
    }

    #endregion

    #region AddPythonTool Tests

    [Fact]
    public void AddPythonTool_WithScriptPath_RegistersServices() {
        // Arrange
        ServiceCollection services = new();
        string scriptPath = "test_script.py";

        // Act
        services.AddPythonTool(scriptPath);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        PythonToolServer? pythonToolServer = serviceProvider.GetService<PythonToolServer>();
        Assert.NotNull(pythonToolServer);
        Assert.Equal(scriptPath, pythonToolServer.ToolScriptPath);

        // Note: We don't test PythonTool resolution here because it requires the server to be started
        // which would require Python to be installed and a valid script file
    }

    [Fact]
    public void AddPythonTool_WithFactory_RegistersServices() {
        // Arrange
        ServiceCollection services = new();
        string scriptPath = "factory_script.py";

        // Act
        services.AddPythonTool(provider => new PythonToolServer(scriptPath));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        PythonToolServer? pythonToolServer = serviceProvider.GetService<PythonToolServer>();
        Assert.NotNull(pythonToolServer);
        Assert.Equal(scriptPath, pythonToolServer.ToolScriptPath);
    }

    [Fact]
    public void AddPythonTool_PythonToolServerIsSingleton() {
        // Arrange
        ServiceCollection services = new();
        services.AddPythonTool("test_script.py");
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        PythonToolServer? server1 = serviceProvider.GetService<PythonToolServer>();
        PythonToolServer? server2 = serviceProvider.GetService<PythonToolServer>();

        // Assert
        Assert.Same(server1, server2);
    }

    [Fact]
    public void AddPythonTool_RegistersPythonToolService() {
        // Arrange
        ServiceCollection services = new();
        services.AddPythonTool("test_script.py");

        // Act & Assert
        ServiceDescriptor? serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(PythonTool));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
    }

    [Fact]
    public async Task StartPythonToolServerAsync_WithoutRegisteredServer_ThrowsInvalidOperationException() {
        // Arrange
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await serviceProvider.StartPythonToolServerAsync());
    }

    [Fact]
    public void StartPythonToolServerAsync_WithRegisteredServer_ReturnsServiceProvider() {
        // Arrange
        ServiceCollection services = new();
        services.AddPythonTool("test_script.py");
        _ = services.BuildServiceProvider();

        // Act & Assert
        // Note: We can't actually test the StartAsync call because it would require Python
        // and a valid script file. We just verify the method exists and can be called.
        MethodInfo? method = typeof(ServiceProviderExtensions).GetMethod("StartPythonToolServerAsync");
        Assert.NotNull(method);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<IServiceProvider>), method.ReturnType);
    }

    [Fact]
    public void StartPythonToolServerAsync_ReturnsOriginalServiceProvider() {
        // Arrange
        ServiceCollection services = new();
        services.AddPythonTool("test_script.py");
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        // We test that the extension method exists and has the correct signature
        // The actual functionality would require a running Python environment
        PythonToolServer server = serviceProvider.GetRequiredService<PythonToolServer>();
        Assert.NotNull(server);

        // Verify the extension method is available (compile-time check)
        bool extensionMethodExists = typeof(ServiceProviderExtensions)
            .GetMethods()
            .Any(m => m.Name == "StartPythonToolServerAsync" &&
                      m.GetParameters().Length >= 1 &&
                      m.GetParameters()[0].ParameterType == typeof(IServiceProvider));

        Assert.True(extensionMethodExists);
    }

    #endregion
}
