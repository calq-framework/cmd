using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.TerminalComponents;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Test;

/// <summary>
///     Integration tests for CalqCmdController using HttpTool from LocalHttpToolFactory
///     Tests the full HTTP pipeline including CLI command execution
/// </summary>
public class CalqCmdControllerIntegrationTest {
    private async Task<HttpTool> CreateHttpToolAsync() {
        IHostBuilder hostBuilder = new HostBuilder().ConfigureWebHost(webHost => {
            webHost.UseTestServer();
            webHost.ConfigureServices(services => {
                services.AddControllers()
                    .AddApplicationPart(typeof(CalqCmdController).Assembly);
                services.AddCalqCmdController(new TestCommandTarget());
            });
            webHost.Configure(app => {
                app.UseRouting();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });
        });

        IHost host = await hostBuilder.StartAsync();
        TestServer server = host.GetTestServer();
        HttpClient testClient = server.CreateClient();

        string serverBaseUrl = testClient.BaseAddress!.ToString()
            .TrimEnd('/');
        string controllerRoute = nameof(CalqCmdController)
            .Replace("Controller", "");
        string fullBaseUrl = $"{serverBaseUrl}/{controllerRoute}";

        LocalHttpToolFactory factory = new(fullBaseUrl, testClient);

        return (HttpTool)factory.CreateLocalTool();
    }

    [Fact]
    public async Task ExecuteScript_WithValidCommand_ReturnsResult() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("TestMethod");

        Assert.Equal("Test CLI output", result);
    }

    [Fact]
    public async Task ExecuteScript_WithParameterizedCommand_ReturnsProcessedResult() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("ProcessData --data test");

        Assert.Equal("Processed: TEST", result);
    }

    [Fact]
    public async Task ExecuteScript_WithIntegerCommand_ReturnsNumericResult() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("Add --a 5 --b 3");

        Assert.Equal("8", result);
    }

    [Fact]
    public async Task ExecuteScript_WithStreamCommand_ReturnsStreamContent() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("GetTestStream");

        Assert.Equal("This is a test stream content", result);
    }

    [Fact]
    public async Task ExecuteScript_WithInvalidCommand_ThrowsException() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        HttpResetTestException exception = Assert.Throws<HttpResetTestException>(() => CMD("NonexistentCommand"));

        Assert.NotNull(exception);
        Assert.Contains("reset the request with error code", exception.Message);
    }

    [Fact]
    public async Task ExecuteScript_WithInputStream_ProcessesInput() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;
        string inputData = "test input data";
        MemoryStream inputStream = new(Encoding.UTF8.GetBytes(inputData));

        string result = CMD("ProcessDataFromStream", inputStream);

        Assert.Equal("Processed: TEST INPUT DATA", result);
    }

    [Fact]
    public async Task ExecuteScript_WithHelpCommand_ReturnsHelpOutput() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("--help");

        Assert.NotEmpty(result);
        Assert.Contains("TestMethod", result);
        Assert.Contains("ProcessData", result);
        Assert.Contains("Add", result);
    }

    [Fact]
    public async Task ExecuteScript_WithSubcommandHelp_ReturnsSubcommandHelpOutput() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("Add --help");

        Assert.NotEmpty(result);
        Assert.Contains("Parameters", result);
        Assert.Contains("-a", result);
        Assert.Contains("-b", result);
        Assert.Contains("int32", result);
    }

    [Fact]
    public async Task ExecuteScript_WithQueryStringScript_ReturnsResult() {
        IHostBuilder hostBuilder = new HostBuilder().ConfigureWebHost(webHost => {
            webHost.UseTestServer();
            webHost.ConfigureServices(services => {
                services.AddControllers()
                    .AddApplicationPart(typeof(CalqCmdController).Assembly);
                services.AddCalqCmdController(new TestCommandTarget());
            });
            webHost.Configure(app => {
                app.UseRouting();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });
        });

        IHost host = await hostBuilder.StartAsync();
        TestServer server = host.GetTestServer();
        HttpClient client = server.CreateClient();

        // Test GET with query string
        HttpResponseMessage response = await client.GetAsync("/CalqCmd?cmd=TestMethod");
        string result = await response.Content.ReadAsStringAsync();

        Assert.Equal("Test CLI output", result);
    }

    [Fact]
    public async Task ExecuteScript_WithQueryStringHelp_ReturnsHelpOutput() {
        IHostBuilder hostBuilder = new HostBuilder().ConfigureWebHost(webHost => {
            webHost.UseTestServer();
            webHost.ConfigureServices(services => {
                services.AddControllers()
                    .AddApplicationPart(typeof(CalqCmdController).Assembly);
                services.AddCalqCmdController(new TestCommandTarget());
            });
            webHost.Configure(app => {
                app.UseRouting();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });
        });

        IHost host = await hostBuilder.StartAsync();
        TestServer server = host.GetTestServer();
        HttpClient client = server.CreateClient();

        // Test GET with --help query string
        HttpResponseMessage response = await client.GetAsync("/CalqCmd?cmd=--help");
        string result = await response.Content.ReadAsStringAsync();

        Assert.NotEmpty(result);
        Assert.Contains("TestMethod", result);
        Assert.Contains("ProcessData", result);
    }

    [Fact]
    public async Task ExecuteScript_WithVoidMethodUsingRUN_WritesToResponseBody() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("VoidMethodWithRUN");

        Assert.NotEmpty(result);
        Assert.Matches(@"^\d+\.\d+\.\d+", result);
    }

    [Fact]
    public async Task ExecuteScript_WithVoidMethodWritingToLocalTerminalOut_WritesToResponseBody() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        string result = CMD("VoidMethodWithDirectOutput");

        Assert.Equal("Direct output", result);
    }

    [Fact]
    public async Task ExecuteScript_WithBinaryInputStream_PreservesBinaryData() {
        HttpTool httpTool = await CreateHttpToolAsync();
        LocalTerminal.Shell = httpTool;

        // Create binary test data with various byte values including null bytes and high-bit-set bytes
        byte[] binaryInput = new byte[256];
        for (int i = 0; i < 256; i++) {
            binaryInput[i] = (byte)i;
        }

        MemoryStream inputStream = new(binaryInput);

        // Use CMDStream to handle binary data directly
        using ShellWorkerOutputStream stream = CMDStream("ProcessBinaryData", inputStream);
        byte[] outputBytes = new byte[256];
        int bytesRead = await stream.ReadAsync(outputBytes);

        Assert.Equal(binaryInput.Length, bytesRead);
        Assert.Equal(binaryInput, outputBytes);
    }


    /// <summary>
    ///     Test command target class with various method signatures for testing different CLI scenarios
    /// </summary>
    private class TestCommandTarget {
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

        public static void VoidMethodWithRUN() {
            LocalTerminal.TerminalLogger = new NullTerminalLogger();
            RUN("dotnet --version");
        }

        public static void VoidMethodWithDirectOutput() {
            StreamWriter sw = new(LocalTerminal.Out);
            sw.Write("Direct output");
            sw.Flush();
        }

        public static async Task ProcessBinaryData() {
            if (LocalTerminal.Shell.In == null) {
                return;
            }

            // Read binary data from input stream
            byte[] buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = await LocalTerminal.Shell.In.ReadAsync(buffer.AsMemory())) > 0) {
                // Write binary data directly to output stream
                await LocalTerminal.Out.WriteAsync(buffer.AsMemory(0, bytesRead));
            }
        }
    }
}
