using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class TerminalTest {
    [Fact]
    public void LocalShellTest() {
        LocalShell = new Cmd.Shells.CommandLine();
        var cmd = LocalShell;
        Task.Run(() => {
            LocalShell = new Bash();
            Assert.True(LocalShell is Bash);
        });
        Assert.Equal(cmd, LocalShell);
    }

    [Fact]
    public void CurrentDirectoryTest() {
        var currentDirectory = LocalShell.CurrentDirectory;
        Task.Run(() => {
            try {
                CD("changed");
            } catch (CommandExecutionException e) {

            }
            Assert.NotEqual(currentDirectory, LocalShell.CurrentDirectory);
        });
        Assert.Equal(currentDirectory, LocalShell.CurrentDirectory);
    }

    [Fact]
    public void CommandLineUtilTest() {
        LocalShell = new Cmd.Shells.CommandLine();
        var output = CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public void BashUtilTest() {
        var writer = new StringWriter();
        var input = "hello world\n";
        Console.SetIn(new StringReader(input));
        Console.SetOut(writer);

        LocalShell = new Bash();
        RUN("sleep 1; read -r input; echo $input");
        var output = writer.ToString();

        Assert.Equal(input, output);
    }

    [Fact]
    public void LongOutputTest() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        Console.SetIn(new StringReader(expectedOutput));
        Console.SetOut(writer);

        LocalShell = new Bash();
        var output = CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public void LongRunOutputTest() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        Console.SetIn(new StringReader(expectedOutput));
        Console.SetOut(writer);

        LocalShell = new Bash();
        RUN("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, writerOutput);
    }
}
