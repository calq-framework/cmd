using CalqFramework.Cmd.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace CalqFramework.Cmd.AspNetCoreTest;

/// <summary>
/// Tests for LocalHttpToolFactory class behavior (constructors, disposal, etc.)
/// </summary>
public class LocalHttpToolFactoryTest
{
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
    public void LocalHttpToolFactory_Dispose_CanBeCalledMultipleTimes()
    {
        var baseUrl = "https://localhost:5000/CalqCmd";
        var factory = new LocalHttpToolFactory(baseUrl);
        
        factory.Dispose();
        factory.Dispose();
        factory.Dispose();
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