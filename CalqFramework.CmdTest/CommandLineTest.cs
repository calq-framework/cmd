using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class CommandLineTest {

    [Fact]
    public void CommandLineWorker_WithSingleWordCommand_ProcessesCorrectly() {
        LocalTerminal.Shell = new CommandLine();

        string output = CMD("whoami");

        Assert.NotEqual("", output);
        Assert.NotNull(output);
    }

    [Fact]
    public void CommandLineWorker_WithCommandAndArguments_ProcessesCorrectly() {
        LocalTerminal.Shell = new CommandLine();

        string output = CMD("dotnet --version");

        Assert.NotEqual("", output);
        Assert.NotNull(output);
    }
}