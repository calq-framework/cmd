using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.TerminalComponents;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Tests;

public class BashTest {
    [Fact]
    public void Bash_ReadInput_EchosCorrectly() {
        MemoryStream writer = new();
        string input = "hello world\n";

        LocalTerminal.Out = writer;
        LocalTerminal.TerminalLogger = new NullTerminalLogger();
        LocalTerminal.Shell = new Bash {
            In = GetStream(input)
        };

        RUN("sleep 1; read -r input; echo $input");
        string output = ReadString(writer);

        Assert.Equal(input, output);
    }

    [Fact]
    public void CMD_WithLongOutput_ReturnsCorrectly() {
        string expectedOutput = "";
        for (int i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        MemoryStream writer = new();
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        string output = CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        string writerOutput = ReadString(writer);

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public void CommandOutput_AfterOutput_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string input = "hello world";

        ShellScript command = CMDV($"echo {input}");
        string output1 = command.Evaluate();
        string output2 = command.Evaluate();

        Assert.Equal(input, output1);
        Assert.Equal(input, output2);
    }

    [Fact]
    public async Task CommandStart_AfterGarbageCollection_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string input = "hello world";

        ShellScript command = CMDV($"sleep 2; echo {input}");
        using IShellWorker proc = await command.StartAsync(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        using StreamReader reader = new(proc.StandardOutput);
        string? output = reader.ReadLine();

        Assert.Equal(input, output);
    }

    private static MemoryStream GetStream(string input) {
        byte[] byteArray = Encoding.ASCII.GetBytes(input);
        MemoryStream stream = new(byteArray);
        return stream;
    }

    [Fact]
    public async Task Bash_BinaryInputOutput_PreservesData() {
        // Create binary test data with various byte values
        byte[] binaryInput = new byte[256];
        for (int i = 0; i < 256; i++) {
            binaryInput[i] = (byte)i;
        }

        MemoryStream inputStream = new(binaryInput);

        LocalTerminal.TerminalLogger = new NullTerminalLogger();
        LocalTerminal.Shell = new Bash();

        // Use CMDStream to handle binary data directly
        using ShellWorkerOutputStream stream = CMDStream("cat", inputStream);
        byte[] outputBytes = new byte[256];
        int bytesRead = await stream.ReadAsync(outputBytes);

        Assert.Equal(binaryInput.Length, bytesRead);
        Assert.Equal(binaryInput, outputBytes);
    }

    private static string ReadString(Stream writer) {
        writer.Position = 0;
        using StreamReader reader = new(writer);
        return reader.ReadToEnd();
    }
}
