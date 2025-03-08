namespace CalqFramework.CmdTest;

public class Test {
    [Fact]
    public void LocalShellTest() {
        ShellUtil.LocalShell = new CommandLine();
        var cmd = ShellUtil.LocalShell;
        Task.Run(() => {
            ShellUtil.LocalShell = new Bash();
            Assert.True(ShellUtil.LocalShell is Bash);
        });
        Assert.Equal(cmd, ShellUtil.LocalShell);
    }

    [Fact]
    public void CurrentDirectoryTest() {
        var currentDirectory = ShellUtil.LocalShell.CurrentDirectory;
        Task.Run(() => {
            ShellUtil.CD("changed");
            Assert.NotEqual(currentDirectory, ShellUtil.LocalShell.CurrentDirectory);
        });
        Assert.Equal(currentDirectory, ShellUtil.LocalShell.CurrentDirectory);
    }

    [Fact]
    public void CommandLineUtilTest() {
        ShellUtil.LocalShell = new CommandLine();
        var output = ShellUtil.CMD("dotnet --version");

        Assert.NotEqual("", output);
    }

    [Fact]
    public void BashUtilTest() {
        var writer = new StringWriter();
        var input = "hello world\n";
        Console.SetIn(new StringReader(input));
        Console.SetOut(writer);

        ShellUtil.LocalShell = new Bash();
        new Bash().RUN("sleep 1; read -r input; echo $input");
        var output = writer.ToString();

        Assert.Equal(input, output);
    }

    [Fact]
    public void LongOutputTest() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        Console.SetIn(new StringReader(expectedOutput));
        Console.SetOut(writer);

        ShellUtil.LocalShell = new Bash();
        var output = new Bash().CMD("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, output);
        Assert.Empty(writerOutput);
    }

    [Fact]
    public void LongRunOutputTest() {
        var expectedOutput = "";
        for (var i = 0; i < 2500; ++i) {
            expectedOutput += "1234567890";
        }

        var writer = new StringWriter();
        Console.SetIn(new StringReader(expectedOutput));
        Console.SetOut(writer);

        ShellUtil.LocalShell = new Bash();
        new Bash().RUN("sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500}; sleep 1; printf '1234567890'%.0s {1..500};");
        var writerOutput = writer.ToString();

        Assert.Equal(expectedOutput, writerOutput);
    }
}
