using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCoreTest;

/// <summary>
/// Integration tests for CalqCmdController using HttpTool from LocalHttpToolFactory
/// Tests the full HTTP pipeline including CLI command execution
/// </summary>
public class CalqCmdControllerIntegrationTest
{

    /// <summary>
    /// Test CLI target class with various method signatures for testing different CLI scenarios
    /// </summary>
    private class TestCliTarget
    {
        public string TestMethod()
        {
            return "Test CLI output";
        }
        
        public string ProcessData(string data)
        {
            return $"Processed: {data.ToUpper()}";
        }
        
        public async Task<string> ProcessDataFromStream()
        {
            if (LocalTerminal.Shell.In == null)
            {
                return "Processed: NO_INPUT";
            }
            
            using var reader = new StreamReader(LocalTerminal.Shell.In);
            var inputData = await reader.ReadToEndAsync();
            return $"Processed: {inputData.Trim().ToUpper()}";
        }
        
        public int Add(int a, int b)
        {
            return a + b;
        }
        
        public Stream GetTestStream()
        {
            var content = "This is a test stream content";
            var bytes = Encoding.UTF8.GetBytes(content);
            return new MemoryStream(bytes);
        }
    }

    private async Task<HttpTool> CreateHttpToolAsync()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddControllers()
                        .AddApplicationPart(typeof(CalqCmdController).Assembly);
                    services.AddCalqCmdController(new TestCliTarget());
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });

        var host = await hostBuilder.StartAsync();
        var server = host.GetTestServer();
        var testClient = server.CreateClient();
        
        var serverBaseUrl = testClient.BaseAddress!.ToString().TrimEnd('/');
        var controllerRoute = nameof(CalqCmdController).Replace("Controller", "");
        var fullBaseUrl = $"{serverBaseUrl}/{controllerRoute}";
        
        var factory = new LocalHttpToolFactory(fullBaseUrl, testClient);
        
        return factory.CreateHttpTool();
    }

    [Fact]
    public async Task ExecuteScript_WithValidCommand_ReturnsResult()
    {
        var httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        
        var result = CMD("test-method");
        
        Assert.Equal("Test CLI output", result);
    }

    [Fact]
    public async Task ExecuteScript_WithParameterizedCommand_ReturnsProcessedResult()
    {
        var httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        
        var result = CMD("process-data --data test");
        
        Assert.Equal("Processed: TEST", result);
    }

    [Fact]
    public async Task ExecuteScript_WithIntegerCommand_ReturnsNumericResult()
    {
        var httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        
        var result = CMD("add --a 5 --b 3");
        
        Assert.Equal("8", result);
    }

    [Fact]
    public async Task ExecuteScript_WithStreamCommand_ReturnsStreamContent()
    {
        var httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        
        var result = CMD("get-test-stream");
        
        Assert.Equal("This is a test stream content", result);
    }

    [Fact]
    public async Task ExecuteScript_WithInvalidCommand_ThrowsException()
    {
        var httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        
        var exception = Assert.Throws<Microsoft.AspNetCore.TestHost.HttpResetTestException>(
            () => CMD("nonexistent-command"));
        
        Assert.NotNull(exception);
        Assert.Contains("reset the request with error code", exception.Message);
    }

    [Fact]
    public async Task ExecuteScript_WithInputStream_ProcessesInput()
    {
        var httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        var inputData = "test input data";
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(inputData));
        
        var result = CMD("process-data-from-stream", inputStream);
        
        Assert.Equal("Processed: TEST INPUT DATA", result);
    }
}