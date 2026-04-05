using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shell.SystemProcess;

namespace CalqFramework.Cmd.Shells;

/// <summary>
///     Command line shell implementation for Windows cmd.exe or Unix shell.
///     On Windows, routes non-exe commands through cmd.exe to resolve .cmd/.bat executables.
/// </summary>
public class CommandLine : ShellBase {
    public override ProcessWorkerBase CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) {
        if (OperatingSystem.IsWindows()) {
            string command = shellScript.Script.Split(' ')[0];
            // Only wrap with cmd.exe if the command isn't a direct .exe
            if (!command.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && !File.Exists(command)) {
                string? resolved = ResolveExe(command);
                if (resolved == null) {
                    var wrapped = new ShellScript(this, $"cmd.exe /c {shellScript.Script}");
                    return new CommandLineWorker(wrapped, inputStream, disposeOnCompletion);
                }
            }
        }
        return new CommandLineWorker(shellScript, inputStream, disposeOnCompletion);
    }

    public override string MapToHostPath(string internalPath) => Path.GetFullPath(internalPath);

    public override string MapToInternalPath(string hostPath) => Path.GetFullPath(hostPath);

    private static string? ResolveExe(string command) {
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        foreach (string dir in paths) {
            string exePath = Path.Combine(dir, command + ".exe");
            if (File.Exists(exePath)) return exePath;
        }
        return null;
    }
}
