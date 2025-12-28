using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class TerminalTest {

    [Fact]
    public void CMD_WithValidCommand_ReturnsNonEmpty() {
        LocalTerminal.Shell = new CommandLine();

        string output = CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public void CMDStream_WithValidCommand_ReturnsStream() {
        LocalTerminal.Shell = new CommandLine();

        using var stream = CMDStream("cmd /c echo hello world");
        using var reader = new StreamReader(stream);
        string output = reader.ReadToEnd().Trim();

        Assert.Equal("hello world", output);
    }

    [Fact]
    public async Task CMDStreamAsync_WithValidCommand_ReturnsStream() {
        LocalTerminal.Shell = new CommandLine();

        using var stream = await CMDStreamAsync("cmd /c echo hello world");
        using var reader = new StreamReader(stream);
        string output = (await reader.ReadToEndAsync()).Trim();

        Assert.Equal("hello world", output);
    }

    [Fact]
    public async Task Shell_WhenChangedInTask_RevertsToOriginal() {
        LocalTerminal.Shell = new CommandLine();
        Cmd.Shell.IShell cmd = LocalTerminal.Shell;
        await Task.Run(() => {
            LocalTerminal.Shell = new CommandLine(); // Changed from Bash to CommandLine
            Assert.True(LocalTerminal.Shell is CommandLine);
        });
        Assert.Equal(cmd, LocalTerminal.Shell);
    }

    [Fact]
    public async Task WorkingDirectory_WhenChangedInTask_RevertsToOriginal() {
        string currentDirectory = PWD;
        await Task.Run(() => {
            CD("changed");
            Assert.NotEqual(currentDirectory, PWD);
        });
        Assert.Equal(currentDirectory, PWD);
    }
}
