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

    // TODO detect Console.SetIn as input redirect or figure out a test with input redirect from file
    //[Fact]
    //public void RedirectedInput() {
    //    var input = "hello world\n";
    //    Console.SetIn(new StringReader(input));

    //    var output = BashUtil.CMD("sleep 1; read -r input; echo $input");

    //    Assert.NotEqual(input, output);
    //}
}
