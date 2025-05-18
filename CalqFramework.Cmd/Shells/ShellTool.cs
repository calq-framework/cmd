using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

public class ShellTool(IShell shell, string command) : ShellBase {
    public string Command { get; init; } = command;
    public IShell Shell { get; } = shell;

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream) {
        shellScript.Script = Command + " " + shellScript.Script;
        shellScript.Shell = Shell;
        return Shell.CreateShellWorker(shellScript, inputStream);
    }

    public override string MapToHostPath(string internalPth) {
        return Shell.MapToHostPath(internalPth);
    }

    public override string MapToInternalPath(string hostPath) {
        return Shell.MapToInternalPath(hostPath);
    }
}
