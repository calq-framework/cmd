namespace CalqFramework.ShellTest;

public class Test
{
    [Fact]
    public void CommandLineUtilTest()
    {
        ShellUtil.SetShell(new CommandLine());
        var output = ShellUtil.CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public void BashUtilTest()
    {
        var input = "hello world\n";
        Console.SetIn(new StringReader(input));

        ShellUtil.SetShell(new Bash());
        var output = new Bash().CMD("sleep 1; read -r input; echo $input");

        Assert.Equal(input, output);
    }
}
