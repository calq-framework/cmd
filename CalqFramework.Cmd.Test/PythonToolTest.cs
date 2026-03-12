using System.Text;
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.TerminalComponents;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Test;

public class PythonToolTest {
    [Fact]
    public async Task PythonTool_ReadInput_EchosCorrectly() {
        string input = "hello world\nhello world\n";
        MemoryStream writer = new();
        LocalTerminal.Out = writer;
        LocalTerminal.TerminalLogger = new NullTerminalLogger();

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
        LocalTerminal.TerminalLogger = new NullTerminalLogger();

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
                int bytesRead = await worker.StandardOutput.ReadAsync(outputBuffer.AsMemory());
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
        Assert.Contains("An intentional error occurred.", errorMessage);
        Assert.True(errorMessage.Length > 10, $"Error message too short: '{errorMessage}'");
    }

    private static MemoryStream GetStream(string input) {
        byte[] byteArray = Encoding.ASCII.GetBytes(input);
        MemoryStream stream = new(byteArray);
        return stream;
    }

    [Fact]
    public async Task PythonTool_BinaryInputOutput_PreservesData() {
        // Create binary test data with various byte values including null bytes and high-bit-set bytes
        byte[] binaryInput = new byte[256];
        for (int i = 0; i < 256; i++) {
            binaryInput[i] = (byte)i;
        }

        MemoryStream inputStream = new(binaryInput);
        MemoryStream outputStream = new();
        
        LocalTerminal.Out = outputStream;
        LocalTerminal.TerminalLogger = new NullTerminalLogger();

        PythonToolServer pythonServer = new("./test_tool.py");
        await pythonServer.StartAsync();
        LocalTerminal.Shell = new PythonTool(pythonServer) {
            In = inputStream
        };

        RUN("test_binary");

        byte[] outputBytes = outputStream.ToArray();
        
        Assert.Equal(binaryInput.Length, outputBytes.Length);
        Assert.Equal(binaryInput, outputBytes);
    }

    private static string ReadString(Stream writer) {
        writer.Position = 0;
        using StreamReader reader = new(writer);
        return reader.ReadToEnd();
    }
}
