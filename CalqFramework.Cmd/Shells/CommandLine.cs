using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shells;
public class CommandLine : ShellBase {

    internal override bool IsUsingWSL => false;

    internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
        int spaceIndex = script.IndexOf(' ');
        var command = script.Substring(0, spaceIndex);
        var arguments = script.Substring(spaceIndex + 1);
        return new ProcessExecutionInfo(command, arguments);
    }
}
