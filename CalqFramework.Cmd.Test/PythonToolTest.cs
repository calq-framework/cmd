using System.Text;
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Test;

public class PythonToolTest {
    [Fact]
    public async Task PythonTool_ReadInput_EchosCorrectly() {
        string input = "hello world\nhello world\n";
        MemoryStream writer = new();
        LocalTerminal.Out = writer;

        PythonToolServer pythonServer = new("./test_tool.py");
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
        MemoryStream writer = new();
        LocalTerminal.Out = writer;

        PythonToolServer pythonServer = new("./test_tool.py");
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

        PythonToolServer pythonServer = new("./test_tool.py");
        await pythonServer.StartAsync();
        PythonTool shell = new(pythonServer);

        ShellScript shellScript = new(shell, "test_throw_exception");
        using IShellWorker worker = await shellScript.StartAsync(GetStream(input), false);

        byte[] outputBuffer = new byte[1024];
        List<byte> totalOutput = [];

        try {
            while (true) {
                int bytesRead = await worker.StandardOutput.ReadAsync(outputBuffer);
                if (bytesRead == 0) {
                    break;
                }

                totalOutput.AddRange(outputBuffer.Take(bytesRead));
            }

            Assert.Fail("Expected an exception but none was thrown");
        } catch (Exception) {
        }

        string output = Encoding.UTF8.GetString([.. totalOutput]);
        Assert.Contains("hello world", output);

        string errorMessage = await worker.ReadErrorMessageAsync();

        Assert.NotNull(errorMessage);
        Assert.NotEmpty(errorMessage);

        Console.WriteLine($"Error message: '{errorMessage}'");

        Assert.True(errorMessage.Length > 10, $"Error message too short: '{errorMessage}'");
    }

    private static MemoryStream GetStream(string input) {
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
