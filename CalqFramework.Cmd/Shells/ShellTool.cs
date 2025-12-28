using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

/// <summary>
/// Shell wrapper that prepends a command to all executed scripts.
/// Useful for sudo, git, docker, and other command prefixes.
/// Example: new ShellTool(new Bash(), "sudo") makes all commands run with sudo.
/// </summary>

public class ShellTool(IShell shell, string command) : ShellBase {
    /// <summary>
    /// Command prefix that will be prepended to all executed scripts.
    /// </summary>
    public string Command { get; init; } = command;
    
    /// <summary>
    /// Underlying shell implementation that will execute the prefixed commands.
    /// </summary>
    public IShell Shell { get; } = shell;

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) {
        shellScript.Script = Command + " " + shellScript.Script;
        shellScript.Shell = Shell;
        return Shell.CreateShellWorker(shellScript, inputStream, disposeOnCompletion);
    }

    public override string MapToHostPath(string internalPth) {
        return Shell.MapToHostPath(internalPth);
    }

    public override string MapToInternalPath(string hostPath) {
        return Shell.MapToInternalPath(hostPath);
    }
}
