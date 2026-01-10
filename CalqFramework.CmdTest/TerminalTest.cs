using CalqFramework.Cmd.Shells;
using System.Text.Json;
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

    [Fact]
    public void CMD_WithJsonOutput_DeserializesCorrectly() {
        LocalTerminal.Shell = new Bash();

        var result = CMD<TestData>("echo '{\"Name\":\"Test\",\"Value\":42}'");

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task CMDAsync_WithJsonOutput_DeserializesCorrectly() {
        LocalTerminal.Shell = new Bash();

        var result = await CMDAsync<TestData>("echo '{\"Name\":\"AsyncTest\",\"Value\":123}'");

        Assert.NotNull(result);
        Assert.Equal("AsyncTest", result.Name);
        Assert.Equal(123, result.Value);
    }

    [Fact]
    public void CMD_WithInputStreamAndJsonOutput_DeserializesCorrectly() {
        LocalTerminal.Shell = new Bash();
        using var inputStream = new MemoryStream();

        var result = CMD<TestData>("echo '{\"Name\":\"InputTest\",\"Value\":999}'", inputStream);

        Assert.NotNull(result);
        Assert.Equal("InputTest", result.Name);
        Assert.Equal(999, result.Value);
    }

    [Fact]
    public async Task CMDAsync_WithInputStreamAndJsonOutput_DeserializesCorrectly() {
        LocalTerminal.Shell = new Bash();
        using var inputStream = new MemoryStream();

        var result = await CMDAsync<TestData>("echo '{\"Name\":\"AsyncInputTest\",\"Value\":777}'", inputStream);

        Assert.NotNull(result);
        Assert.Equal("AsyncInputTest", result.Name);
        Assert.Equal(777, result.Value);
    }

    [Fact]
    public void CMD_WithInvalidJson_ThrowsJsonException() {
        LocalTerminal.Shell = new Bash();

        Assert.Throws<JsonException>(() => CMD<TestData>("echo 'invalid-json'"));
    }

    [Fact]
    public async Task CMDAsync_WithInvalidJson_ThrowsJsonException() {
        LocalTerminal.Shell = new Bash();

        await Assert.ThrowsAsync<JsonException>(() => CMDAsync<TestData>("echo 'invalid-json'"));
    }

    [Fact]
    public void CMD_WithNullJson_ReturnsNull() {
        LocalTerminal.Shell = new Bash();

        var result = CMD<TestData>("echo 'null'");

        Assert.Null(result);
    }

    [Fact]
    public async Task CMDAsync_WithNullJson_ReturnsNull() {
        LocalTerminal.Shell = new Bash();

        var result = await CMDAsync<TestData>("echo 'null'");

        Assert.Null(result);
    }

    private class TestData {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
