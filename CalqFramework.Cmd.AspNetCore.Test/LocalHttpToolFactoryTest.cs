namespace CalqFramework.Cmd.AspNetCore.Test;

/// <summary>
///     Tests for LocalHttpToolFactory class behavior (constructors, disposal, etc.)
/// </summary>
public class LocalHttpToolFactoryTest {
    [Fact]
    public void LocalHttpToolFactory_ExplicitConstructor_WithProvidedHttpClient_DoesNotOwnClient() {
        using HttpClient httpClient = new();
        string baseUrl = "https://localhost:5000/CalqCmd";

        using LocalHttpToolFactory factory = new(baseUrl, httpClient);

        Assert.NotNull(factory);

        factory.Dispose();

        httpClient.Timeout = TimeSpan.FromSeconds(30);
        Assert.Equal(TimeSpan.FromSeconds(30), httpClient.Timeout);
    }

    [Fact]
    public void LocalHttpToolFactory_ExplicitConstructor_WithoutHttpClient_CreatesNewClient() {
        string baseUrl = "https://localhost:5000/CalqCmd";

        using LocalHttpToolFactory factory = new(baseUrl);
        Assert.NotNull(factory);
    }

    [Fact]
    public void LocalHttpToolFactory_Dispose_CanBeCalledMultipleTimes() {
        string baseUrl = "https://localhost:5000/CalqCmd";
        LocalHttpToolFactory factory = new(baseUrl);

        factory.Dispose();
        factory.Dispose();
        factory.Dispose();
    }

    [Fact]
    public void LocalHttpToolFactory_AfterDispose_ThrowsObjectDisposedException() {
        string baseUrl = "https://localhost:5000/CalqCmd";
        LocalHttpToolFactory factory = new(baseUrl);
        factory.Dispose();

        Assert.Throws<ObjectDisposedException>(() => factory.CreateLocalTool());
    }
}
