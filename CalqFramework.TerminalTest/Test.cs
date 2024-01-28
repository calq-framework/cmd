namespace CalqFramework.TerminalTest;

public class Test
{
   [Fact]
    public void BashUtilTest()
    {
        var input = "hello world\n";
        Console.SetIn(new StringReader(input));

        var output = BashUtil.CMD("sleep 1; read -r input; echo $input");

        Assert.Equal(input, output);
    }

    [Fact]
    public void CommandLineUtilTest()
    {
        var output = CommandLineUtil.CMD("dotnet --version");

        Assert.NotEqual("", output);
    }
}
