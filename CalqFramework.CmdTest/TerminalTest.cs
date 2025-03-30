using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class TerminalTest {
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
        var currentDirectory = LocalTerminal.ShellCommandRunConfiguration.WorkingDirectory;
        await Task.Run(() => {
            CD("changed");
            Assert.NotEqual(currentDirectory, LocalTerminal.ShellCommandRunConfiguration.WorkingDirectory);
        });
        Assert.Equal(currentDirectory, LocalTerminal.ShellCommandRunConfiguration.WorkingDirectory);
    }

    [Fact]
    public void CMD_WithValidCommand_ReturnsNonEmpty() {
        LocalTerminal.Shell = new CommandLine();

        var output = CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public void Bash_ReadInput_EchosCorrectly() {
        var writer = new StringWriter();
        var input = "hello world\n";

        LocalTerminal.ShellCommandRunConfiguration.In = new StringReader(input);
        LocalTerminal.ShellCommandRunConfiguration.Out = writer;
        LocalTerminal.Shell = new Bash();

        RUN("sleep 1; read -r input; echo $input");
        var output = writer.ToString();

        Assert.Equal(input, string.Join("\n", output.Split('\n').Skip(3))); // skip log output
    }

    [Fact]
    public void CMD_WithLongOutput_ReturnsCorrectly() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        LocalTerminal.ShellCommandRunConfiguration.In = new StringReader(expectedOutput);
        LocalTerminal.ShellCommandRunConfiguration.Out = writer;
        LocalTerminal.Shell = new Bash();

        var output = CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public void RUN_WithLongOutput_WritesCorrectly() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        LocalTerminal.ShellCommandRunConfiguration.In = new StringReader(expectedOutput);
        LocalTerminal.ShellCommandRunConfiguration.Out = writer;
        LocalTerminal.Shell = new Bash();

        RUN("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, string.Join("\n", writerOutput.Split('\n').Skip(3))); // skip log output
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
    public void CommandStart_AfterGarbageCollection_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        var input = "hello world";

        var command = CMDV($"sleep 2; echo {input}");
        using var proc = command.Start();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var output = proc.StandardOutput.ReadLine();

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

        Assert.Throws<ShellCommandExecutionException>(() => {
            string output = CMDV($"echo {echoText}") | CMDV("cat; exit 1;") | CMDV("cat");
        });
    }
}
