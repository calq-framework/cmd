using System.Text;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class HttpToolTest {

    [Fact]
    public async Task HttpTool_EchoContentBody_ReturnsCorrectly() {
        LocalTerminal.Shell = new CommandLine();
        ShellScript pythonScript = CMDV("python http_echo_server.py");
        using Cmd.Shell.IShellWorker serverWorker = await pythonScript.StartAsync();

        // Give the server time to start
        await Task.Delay(1000);

        var httpClient = new HttpClient {
            BaseAddress = new Uri("http://127.0.0.1:8001")
        };
        string input = "hello world";
        LocalTerminal.Shell = new HttpTool(httpClient) {
            In = GetStream(input)
        };

        string echo = CMD("", LocalTerminal.Shell.In);

        Assert.Equal(input, echo);
    }

    [Fact]
    public async Task HttpTool_EvalPython_ReturnsCorrectly() {
        LocalTerminal.Shell = new CommandLine();
        ShellScript pythonScript = CMDV("python http_eval_server.py");
        using Cmd.Shell.IShellWorker serverWorker = await pythonScript.StartAsync();

        // Give the server time to start
        await Task.Delay(1000);

        var httpClient = new HttpClient {
            BaseAddress = new Uri("http://127.0.0.1:8000")
        };
        LocalTerminal.Shell = new HttpTool(httpClient);

        string echo = CMD("sum([8, 16, 32])");

        Assert.Equal((8 + 16 + 32).ToString(), echo);
    }

    [Fact]
    public async Task HttpTool_MidStreamError_Throws() {
        LocalTerminal.Shell = new CommandLine();
        CMD("openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes -subj \"/CN=localhost\"");
        ShellScript pythonScript = CMDV("python http_h2_server.py");
        using Cmd.Shell.IShellWorker serverWorker = await pythonScript.StartAsync();

        // Give the server time to start
        await Task.Delay(2000);

        var handler = new HttpClientHandler {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://localhost:8443")
        };

        string inputBeforeReset = "hello\nworld\n";
        string input = inputBeforeReset + "should not be streamed back\n";
        LocalTerminal.Shell = new HttpTool(httpClient) {
            In = GetStream(input)
        };

        ShellScript echo = CMDV("");
        using Cmd.Shell.IShellWorker requestWorker = await echo.StartAsync(disposeOnCompletion: false);
        var reader = new StreamReader(requestWorker.StandardOutput);
        string output = "";
        Assert.Throws<ShellWorkerException>(() => {
            output += reader.ReadLine() + '\n'; // ok
            output += reader.ReadLine() + '\n'; // ok
            output += reader.ReadLine() + '\n'; // throws
        });

        Assert.Equal(inputBeforeReset, output);
    }

    private static Stream GetStream(string input) {
        byte[] byteArray = Encoding.ASCII.GetBytes(input);
        MemoryStream stream = new(byteArray);
        return stream;
    }

    private static string ReadString(Stream writer) {
        writer.Position = 0;
        using StreamReader reader = new(writer);
        return reader.ReadToEnd();
    }
}