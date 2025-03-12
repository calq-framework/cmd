using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class TerminalTest {
    [Fact]
    public async void LocalShellTest() {
        LocalTerminal.Shell = new CommandLine();
        var cmd = LocalTerminal.Shell;
        await Task.Run(() => {
            LocalTerminal.Shell = new Bash();
            Assert.True(LocalTerminal.Shell is Bash);
        });
        Assert.Equal(cmd, LocalTerminal.Shell);
    }

    [Fact]
    public async void CurrentDirectoryTest() {
        var currentDirectory = LocalTerminal.WorkingDirectory;
        await Task.Run(() => {
            CD("changed");
            Assert.NotEqual(currentDirectory, LocalTerminal.WorkingDirectory);
        });
        Assert.Equal(currentDirectory, LocalTerminal.WorkingDirectory);
    }

    [Fact]
    public async void CommandLineUtilTest() {
        LocalTerminal.Shell = new CommandLine();

        var output = CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public async void BashUtilTest() {
        var writer = new StringWriter();
        var input = "hello world\n";

        LocalTerminal.In = new StringReader(input);
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        RUN("sleep 1; read -r input; echo $input");
        var output = writer.ToString();

        Assert.Equal(input, output);
    }

    [Fact]
    public async void LongOutputTest() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        LocalTerminal.In = new StringReader(expectedOutput);
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        var output = CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public async void LongRunOutputTest() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        LocalTerminal.In = new StringReader(expectedOutput);
        LocalTerminal.Out = writer;
        LocalTerminal.Shell = new Bash();

        RUN("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, writerOutput);
    }

    [Fact]
    public async void CmdTest() {
        LocalTerminal.Shell = new Bash();
        var echoText = "hello, world";
        var echoCommand = CMD($"echo {echoText}");

        if (string.Compare($"{echoText}\n", echoCommand) != 0) {
            Assert.True(false);
        }
        if ($"{echoText}\n" != echoCommand) {
            Assert.True(false);
        }

        string output = echoCommand | CMD("cut -d',' -f1");
        Assert.Equal("hello\n", output);
    }
}
