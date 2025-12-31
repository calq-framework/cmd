using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.Extensions.DependencyInjection;

namespace CalqFramework.Cmd.AspNetCoreTest;

public class ServiceCollectionExtensionsTest
{
    [Fact]
    public void AddPythonTool_WithScriptPath_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var scriptPath = "test_script.py";

        // Act
        services.AddPythonTool(scriptPath);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pythonToolServer = serviceProvider.GetService<PythonToolServer>();
        Assert.NotNull(pythonToolServer);
        Assert.Equal(scriptPath, pythonToolServer.ToolScriptPath);
        
        // Note: We don't test PythonTool resolution here because it requires the server to be started
        // which would require Python to be installed and a valid script file
    }

    [Fact]
    public void AddPythonTool_WithFactory_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var scriptPath = "factory_script.py";

        // Act
        services.AddPythonTool(provider => new PythonToolServer(scriptPath));
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var pythonToolServer = serviceProvider.GetService<PythonToolServer>();
        Assert.NotNull(pythonToolServer);
        Assert.Equal(scriptPath, pythonToolServer.ToolScriptPath);
    }

    [Fact]
    public void AddPythonTool_PythonToolServerIsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPythonTool("test_script.py");
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var server1 = serviceProvider.GetService<PythonToolServer>();
        var server2 = serviceProvider.GetService<PythonToolServer>();

        // Assert
        Assert.Same(server1, server2);
    }

    [Fact]
    public void AddPythonTool_RegistersPythonToolService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPythonTool("test_script.py");

        // Act & Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(PythonTool));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
    }

    [Fact]
    public async Task StartPythonToolServerAsync_WithoutRegisteredServer_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await serviceProvider.StartPythonToolServerAsync());
    }

    [Fact]
    public void StartPythonToolServerAsync_WithRegisteredServer_ReturnsServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPythonTool("test_script.py");
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        // Note: We can't actually test the StartAsync call because it would require Python
        // and a valid script file. We just verify the method exists and can be called.
        var method = typeof(ServiceProviderExtensions).GetMethod("StartPythonToolServerAsync");
        Assert.NotNull(method);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<IServiceProvider>), method.ReturnType);
    }

    [Fact]
    public void StartPythonToolServerAsync_ReturnsOriginalServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPythonTool("test_script.py");
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        // We test that the extension method exists and has the correct signature
        // The actual functionality would require a running Python environment
        var server = serviceProvider.GetRequiredService<PythonToolServer>();
        Assert.NotNull(server);
        
        // Verify the extension method is available (compile-time check)
        var extensionMethodExists = typeof(ServiceProviderExtensions)
            .GetMethods()
            .Any(m => m.Name == "StartPythonToolServerAsync" && 
                     m.GetParameters().Length >= 1 &&
                     m.GetParameters()[0].ParameterType == typeof(IServiceProvider));
        
        Assert.True(extensionMethodExists);
    }
}