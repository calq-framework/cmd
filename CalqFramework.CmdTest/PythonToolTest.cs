using System.Text;
using CalqFramework.Cmd;
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class PythonToolTest {

    [Fact]
    public async Task PythonTool_ReadInput_EchosCorrectly() {
        string input = "hello world\nhello world\n";
        var writer = new MemoryStream();
        LocalTerminal.Out = writer;

        var pythonServer = new PythonToolServer("./test_tool.py");
        await pythonServer.StartAsync();
        LocalTerminal.Shell = new PythonTool(pythonServer) {
            In = GetStream(input)
        };

        RUN("test");

        string output = ReadString(writer);
        Assert.Equal(input, output);
    }

    [Fact]
    public async Task PythonTool_ThrowException_ReturnsErrorMessage() {
        string input = "hello world\nhello world\n";
        var writer = new MemoryStream();
        LocalTerminal.Out = writer;

        var pythonServer = new PythonToolServer("./test_tool.py");
        await pythonServer.StartAsync();
        LocalTerminal.Shell = new PythonTool(pythonServer) {
            In = GetStream(input)
        };

        Assert.Throws<ShellScriptException>(() => RUN("test_throw_exception"));

        string output = ReadString(writer);
        Assert.Contains("hello world", output);
    }

    [Fact]
    public async Task PythonTool_ThrowException_ReadErrorMessageAsync_ReturnsDetailedError() {
        string input = "hello world\nhello world\n";

        var pythonServer = new PythonToolServer("./test_tool.py");
        await pythonServer.StartAsync();
        var shell = new PythonTool(pythonServer);

        var shellScript = new ShellScript(shell, "test_throw_exception");
        using var worker = await shellScript.StartAsync(GetStream(input));

        var outputBuffer = new byte[1024];
        var totalOutput = new List<byte>();

        try {
            while (true) {
                int bytesRead = await worker.StandardOutput.ReadAsync(outputBuffer, 0, outputBuffer.Length);
                if (bytesRead == 0) break;

                totalOutput.AddRange(outputBuffer.Take(bytesRead));
            }

            Assert.Fail("Expected an exception but none was thrown");
        } catch (Exception) {
        }

        string output = Encoding.UTF8.GetString(totalOutput.ToArray());
        Assert.Contains("hello world", output);

        string errorMessage = await worker.ReadErrorMessageAsync();

        Assert.NotNull(errorMessage);
        Assert.NotEmpty(errorMessage);

        Console.WriteLine($"Error message: '{errorMessage}'");

        Assert.True(errorMessage.Length > 10, $"Error message too short: '{errorMessage}'");
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
