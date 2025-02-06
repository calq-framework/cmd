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

    [Fact]
    public void LongOutputTest()
    {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        Console.SetIn(new StringReader(expectedOutput));
        Console.SetOut(writer);

        ShellUtil.SetShell(new Bash());
        var output = new Bash().CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, output);
        Assert.Equal(expectedOutput, writerOutput);
    }
}
