namespace CalqFramework.Cmd;
public class CommandLine : ShellBase {
    public override string GetLocalPath(string path) {
        return path;
    }

    internal override ScriptExecutionInfo GetScriptExecutionInfo(string script) {
        int spaceIndex = script.IndexOf(' ');
        var command = script.Substring(0, spaceIndex);
        var arguments = script.Substring(spaceIndex + 1);
        return new ScriptExecutionInfo(command, arguments);
    }
}
