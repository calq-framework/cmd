using CalqFramework.Cmd.Execution;

namespace CalqFramework.Cmd.Shells;
public class CommandLine : ShellBase {
    public override string GetLocalPath(string path) {
        return path;
    }

    internal override ProcessExecutionInfo GetProcessExecutionInfo(string script) {
        int spaceIndex = script.IndexOf(' ');
        var command = script.Substring(0, spaceIndex);
        var arguments = script.Substring(spaceIndex + 1);
        return new ProcessExecutionInfo(command, arguments);
    }
}
