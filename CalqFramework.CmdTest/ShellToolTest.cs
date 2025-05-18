using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class ShellToolTest {

    [Fact]
    public void ShellTool_WithValidCommand_ReturnsNonEmpty() {
        LocalTerminal.Shell = new ShellTool(new CommandLine(), "dotnet");

        var output = CMD("--version");

        Assert.NotEqual("", output);
    }
}