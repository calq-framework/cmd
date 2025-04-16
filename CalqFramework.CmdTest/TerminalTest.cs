using CalqFramework.Cmd.Shells;
using System.Text;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class TerminalTest {
    private Stream GetStream(string input) {
        byte[] byteArray = Encoding.ASCII.GetBytes(input);
        MemoryStream stream = new MemoryStream(byteArray);
        return stream;
    }

    private string ReadString(Stream writer) {
        writer.Position = 0;
        using StreamReader reader = new StreamReader(writer);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task Shell_WhenChangedInTask_RevertsToOriginal() {
        LocalTerminal.Shell = new CommandLine();
        var cmd = LocalTerminal.Shell;
        await Task.Run(() => {
            LocalTerminal.Shell = new Bash();
            Assert.True(LocalTerminal.Shell is Bash);
        });
        Assert.Equal(cmd, LocalTerminal.Shell);
    }

    [Fact]
    public async Task WorkingDirectory_WhenChangedInTask_RevertsToOriginal() {
        var currentDirectory = PWD;
        await Task.Run(() => {
            CD("changed");
            Assert.NotEqual(currentDirectory, PWD);
        });
        Assert.Equal(currentDirectory, PWD);
    }

    [Fact]
    public void CMD_WithValidCommand_ReturnsNonEmpty() {
        LocalTerminal.Shell = new CommandLine();

        var output = CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public void Bash_ReadInput_EchosCorrectly() {
        var writer = new MemoryStream();
        var input = "hello world\n";

        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash() {
            In = GetStream(input)
        };

        RUN("sleep 1; read -r input; echo $input");
        var output = ReadString(writer);

        Assert.Equal(input, output);
    }

    [Fact]
    public void CMD_WithLongOutput_ReturnsCorrectly() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new MemoryStream();
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        var output = CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = ReadString(writer);

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public void RUN_WithLongOutput_WritesCorrectly() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new MemoryStream();
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        RUN("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = ReadString(writer);

        Assert.Equal(expectedOutput, writerOutput);
    }

    [Fact]
    public void CommandPiping_WithEchoAndCut_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        var echoText = "hello, world";
        var echoCommand = CMDV($"echo {echoText}");

        if (string.Compare(echoText, echoCommand) != 0) {
            Assert.True(false);
        }
        if (echoText != echoCommand) {
            Assert.True(false);
        }

        string output = echoCommand | CMDV("cut -d',' -f1");
        Assert.Equal("hello", output);
    }

    [Fact]
    public void CommandPiping_AfterMultiplePipes_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        var echoText = "hello world";

        string output = CMDV($"echo {echoText}") | CMDV("cat") | CMDV("cat") | CMDV("cat");
        Assert.Equal(echoText, output);
    }

    [Fact]
    public async void CommandStart_AfterGarbageCollection_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        var input = "hello world";

        var command = CMDV($"sleep 2; echo {input}");
        using var proc = await command.Start();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        using var reader = new StreamReader(proc.StandardOutput);
        var output = reader.ReadLine();

        Assert.Equal(input, output);
    }

    [Fact]
    public void CommandOutput_AfterOutput_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        var input = "hello world";

        var command = CMDV($"echo {input}");
        var output1 = command.Evaluate();
        var output2 = command.Evaluate();

        Assert.Equal(input, output1);
        Assert.Equal(input, output2);
    }

    [Fact]
    public void CommandPiping_WithError_ThrowsException() {
        LocalTerminal.Shell = new Bash();
        var echoText = "hello world";

        Assert.Throws<ShellScriptExecutionException>(() => {
            string output = CMDV($"echo {echoText}") | CMDV("cat; exit 1;") | CMDV("cat");
        });
    }

    [Fact]
    public async void HttpShell_EvalPython_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        var pythonScript = CMDV(@"python <<EOF
from http.server import BaseHTTPRequestHandler, HTTPServer

class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        try:
            script_param = self.headers.get('Script')
            if script_param:
                result = str(eval(script_param))
                self.send_response(200)
                self.send_header('Content-Type', 'text/plain')
                self.end_headers()
                self.wfile.write(result.encode())
            else:
                self.send_response(400)
                self.send_header('Content-Type', 'text/plain')
                self.end_headers()
                self.wfile.write(b'Missing Script header')
        except Exception as e:
            print(""Error during eval:"", e)
            self.send_response(500)
            self.send_header('Content-Type', 'text/plain')
            self.end_headers()
            self.wfile.write(f""Error: {e}"".encode())

HTTPServer(('', 8000), Handler).serve_forever()
EOF
");
        using var serverWorker = await pythonScript.Start();


        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://127.0.0.1:8000");
        LocalTerminal.Shell = new HttpShell(httpClient);

        var echo = CMD("sum([8, 16, 32])");

        Assert.Equal((8 + 16 + 32).ToString(), echo);
    }

    [Fact]
    public async void HttpShell_EchoContentBody_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        var pythonScript = CMDV(@"python <<EOF
from http.server import BaseHTTPRequestHandler, HTTPServer

class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        try:
            content_length = int(self.headers.get('Content-Length', 0))
            post_body = self.rfile.read(content_length).decode('utf-8')

            print(""Received POST body:"")
            print(post_body)

            self.send_response(200)
            self.send_header('Content-Type', 'text/plain')
            self.end_headers()
            self.wfile.write(post_body.encode())
        except Exception as e:
            print(""Error reading POST body:"", e)
            self.send_response(500)
            self.send_header('Content-Type', 'text/plain')
            self.end_headers()
            self.wfile.write(f""Error: {e}"".encode())

HTTPServer(('', 8001), Handler).serve_forever()
EOF
    ");
        using var serverWorker = await pythonScript.Start();


        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://127.0.0.1:8001");
        var input = "hello world";
        LocalTerminal.Shell = new HttpShell(httpClient) {
            In = GetStream(input)
        };

        var echo = CMD("", LocalTerminal.Shell.In);

        Assert.Equal(input, echo);
    }
}
