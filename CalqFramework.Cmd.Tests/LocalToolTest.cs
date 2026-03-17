using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Tests;

public class LocalToolTest {
    [Fact]
    public void LocalTool_WithSingleArgument_ExecutesCurrentExecutable() {
        LocalTerminal.Shell = new LocalTool();

        ShellScriptException exception = Assert.Throws<ShellScriptException>(() => CMD("--version"));

        Assert.Contains("testhost.dll --version", exception.Message);
    }

    [Fact]
    public void LocalTool_WithMultipleArguments_ExecutesCurrentExecutable() {
        LocalTerminal.Shell = new LocalTool();

        ShellScriptException exception = Assert.Throws<ShellScriptException>(() => CMD("--help"));

        Assert.Contains("testhost.dll --help", exception.Message);
    }
}
