namespace CalqFramework.Shell;
public class CommandLine : ShellBase {

    internal override ScriptExecutionInfo GetScriptExecutionInfo(string script) {
        int spaceIndex = script.IndexOf(' ');
        var command = script.Substring(0, spaceIndex);
        var arguments = script.Substring(spaceIndex + 1);
        return new ScriptExecutionInfo(command, arguments);
    }
}
