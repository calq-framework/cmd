using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.CmdTest;

public class ShellPipingTest {

    [Fact]
    public void CommandPiping_AfterMultiplePipes_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string echoText = "hello world";

        string output = CMDV($"echo {echoText}") | CMDV("cat") | CMDV("cat") | CMDV("cat");
        Assert.Equal(echoText, output);
    }

    [Fact]
    public void CommandPiping_WithEchoAndCut_ReturnsCorrectly() {
        LocalTerminal.Shell = new Bash();
        string echoText = "hello, world";
        ShellScript echoCommand = CMDV($"echo {echoText}");

        if (string.Compare(echoText, echoCommand) != 0) {
            Assert.True(false);
        }
        if (echoText != echoCommand) {
            Assert.True(false);
        }

        string output = echoCommand | CMDV("cut -d',' -f1");
        Assert.Equal("hello", output);
    }

    [Fact]
    public void CommandPiping_WithError_ThrowsException() {
        LocalTerminal.Shell = new Bash();
        string echoText = "hello world";

        Assert.Throws<ShellScriptException>(() => {
            string output = CMDV($"echo {echoText}") | CMDV("cat; exit 1;") | CMDV("cat");
        });
    }
}