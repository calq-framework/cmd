using System.Text;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Test;

/// <summary>
///     Integration tests for CalqCmdController using HttpTool from LocalHttpToolFactory
///     Tests the full HTTP pipeline including CLI command execution
/// </summary>
public class CalqCmdControllerIntegrationTest {
    private async Task<HttpTool> CreateHttpToolAsync() {
        IHostBuilder hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost => {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => {
                    services.AddControllers()
                        .AddApplicationPart(typeof(CalqCmdController).Assembly);
                    services.AddCalqCmdController(new TestCliTarget());
                });
                webHost.Configure(app => {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                });
            });

        IHost host = await hostBuilder.StartAsync();
        TestServer server = host.GetTestServer();
        HttpClient testClient = server.CreateClient();

        string serverBaseUrl = testClient.BaseAddress!.ToString().TrimEnd('/');
        string controllerRoute = nameof(CalqCmdController).Replace("Controller", "");
        string fullBaseUrl = $"{serverBaseUrl}/{controllerRoute}";

        LocalHttpToolFactory factory = new(fullBaseUrl, testClient);

        return (HttpTool)factory.CreateLocalTool();
    }

    [Fact]
    public async Task ExecuteScript_WithValidCommand_ReturnsResult() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("test-method");

        Assert.Equal("Test CLI output", result);
    }

    [Fact]
    public async Task ExecuteScript_WithParameterizedCommand_ReturnsProcessedResult() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("process-data --data test");

        Assert.Equal("Processed: TEST", result);
    }

    [Fact]
    public async Task ExecuteScript_WithIntegerCommand_ReturnsNumericResult() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("add --a 5 --b 3");

        Assert.Equal("8", result);
    }

    [Fact]
    public async Task ExecuteScript_WithStreamCommand_ReturnsStreamContent() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("get-test-stream");

        Assert.Equal("This is a test stream content", result);
    }

    [Fact]
    public async Task ExecuteScript_WithInvalidCommand_ThrowsException() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        HttpResetTestException exception = Assert.Throws<HttpResetTestException>(() => CMD("nonexistent-command"));

        Assert.NotNull(exception);
        Assert.Contains("reset the request with error code", exception.Message);
    }

    [Fact]
    public async Task ExecuteScript_WithInputStream_ProcessesInput() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        string inputData = "test input data";
        MemoryStream inputStream = new(Encoding.UTF8.GetBytes(inputData));

        string result = CMD("process-data-from-stream", inputStream);

        Assert.Equal("Processed: TEST INPUT DATA", result);
    }

    /// <summary>
    ///     Test CLI target class with various method signatures for testing different CLI scenarios
    /// </summary>
    private class TestCliTarget {
        public static string TestMethod() => "Test CLI output";

        public static string ProcessData(string data) => $"Processed: {data.ToUpper()}";

        public static async Task<string> ProcessDataFromStream() {
            if (LocalTerminal.Shell.In == null) {
                return "Processed: NO_INPUT";
            }

            using StreamReader reader = new(LocalTerminal.Shell.In);
            string inputData = await reader.ReadToEndAsync();
            return $"Processed: {inputData.Trim().ToUpper()}";
        }

        public static int Add(int a, int b) => a + b;

        public static MemoryStream GetTestStream() {
            string content = "This is a test stream content";
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return new MemoryStream(bytes);
        }
    }
}
