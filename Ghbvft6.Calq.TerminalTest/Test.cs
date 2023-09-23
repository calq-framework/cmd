namespace Ghbvft6.Calq.TerminalTest;

public class Test
{
    [Fact]
    public void BashEcho()
    {
        var output = BashUtil.CMD("echo hello world");

        Assert.Equal("hello world", output.Trim());
    }

    [Fact]
    public void ShellCommand() {
        var output = ShellUtil.CMD("dotnet --version");

        Assert.NotEqual("", output.Trim());
    }
}