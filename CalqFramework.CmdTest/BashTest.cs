using System.Text;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class BashTest {

    [Fact]
    public void Bash_ReadInput_EchosCorrectly() {
        var writer = new MemoryStream();
        string input = "hello world\n";

        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash() {
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

        var writer = new MemoryStream();
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        string output = CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        string writerOutput = ReadString(writer);

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public void RUN_WithLongOutput_WritesCorrectly() {
        string expectedOutput = "";
        for (int i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new MemoryStream();
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        RUN("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        string writerOutput = ReadString(writer);

        Assert.Equal(expectedOutput, writerOutput);
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
        using Cmd.Shell.IShellWorker proc = await command.StartAsync(disposeOnCompletion: false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        using var reader = new StreamReader(proc.StandardOutput);
        string? output = reader.ReadLine();

        Assert.Equal(input, output);
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