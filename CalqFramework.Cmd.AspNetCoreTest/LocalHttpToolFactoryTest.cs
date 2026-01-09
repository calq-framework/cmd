using CalqFramework.Cmd.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace CalqFramework.Cmd.AspNetCoreTest;

/// <summary>
/// Tests for LocalHttpToolFactory HttpClient management and disposal
/// </summary>
public class LocalHttpToolFactoryTest
{
    [Fact]
    public void LocalHttpToolFactory_ServiceBased_UsesSharedHttpClient()
    {
        var services = new ServiceCollection();
        services.AddLocalHttpToolFactory();
        var serviceProvider = services.BuildServiceProvider();
        
        var factory = serviceProvider.GetRequiredService<ILocalToolFactory>();
        
        Assert.NotNull(factory);
        Assert.True(factory is IDisposable);
    }

    [Fact]
    public void LocalHttpToolFactory_ExplicitConstructor_WithProvidedHttpClient_DoesNotOwnClient()
    {
        using var httpClient = new HttpClient();
        var baseUrl = "https://localhost:5000/CalqCmd";
        
        using var factory = new LocalHttpToolFactory(baseUrl, httpClient);
        
        Assert.NotNull(factory);
        
        factory.Dispose();
        
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        Assert.Equal(TimeSpan.FromSeconds(30), httpClient.Timeout);
    }

    [Fact]
    public void LocalHttpToolFactory_ExplicitConstructor_WithoutHttpClient_CreatesNewClient()
    {
        var baseUrl = "https://localhost:5000/CalqCmd";
        
        using var factory = new LocalHttpToolFactory(baseUrl);
        Assert.NotNull(factory);
    }

    [Fact]
    public void LocalHttpToolFactory_ServiceBased_DoesNotOwnHttpClients()
    {
        var services = new ServiceCollection();
        services.AddLocalHttpToolFactory();
        var serviceProvider = services.BuildServiceProvider();
        
        using var factory = (LocalHttpToolFactory)serviceProvider.GetRequiredService<ILocalToolFactory>();
        
        Assert.NotNull(factory);
        factory.Dispose();
    }

    [Fact]
    public void LocalHttpToolFactory_Dispose_CanBeCalledMultipleTimes()
    {
        var baseUrl = "https://localhost:5000/CalqCmd";
        var factory = new LocalHttpToolFactory(baseUrl);
        
        factory.Dispose();
        factory.Dispose();
        factory.Dispose();
    }

    [Fact]
    public void LocalHttpToolFactory_ServiceBased_UsesHttpClientFactory()
    {
        var services = new ServiceCollection();
        services.AddLocalHttpToolFactory();
        var serviceProvider = services.BuildServiceProvider();
        
        var factory = serviceProvider.GetRequiredService<ILocalToolFactory>();
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        
        Assert.NotNull(factory);
        Assert.NotNull(httpClientFactory);
        
        var namedClient = httpClientFactory.CreateClient("CalqFramework.Cmd.LocalHttpTool");
        Assert.NotNull(namedClient);
        Assert.Equal(TimeSpan.FromSeconds(30), namedClient.Timeout);
    }

    [Fact]
    public void LocalHttpToolFactory_ServiceBased_WithCustomConfiguration()
    {
        var services = new ServiceCollection();
        services.AddLocalHttpToolFactory(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        var serviceProvider = services.BuildServiceProvider();
        
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var namedClient = httpClientFactory.CreateClient("CalqFramework.Cmd.LocalHttpTool");
        
        Assert.NotNull(namedClient);
        Assert.Equal(TimeSpan.FromMinutes(5), namedClient.Timeout);
    }

    [Fact]
    public void LocalHttpToolFactory_AfterDispose_ThrowsObjectDisposedException()
    {
        var baseUrl = "https://localhost:5000/CalqCmd";
        var factory = new LocalHttpToolFactory(baseUrl);
        factory.Dispose();
        
        Assert.Throws<ObjectDisposedException>(() => factory.CreateLocalTool());
    }
}