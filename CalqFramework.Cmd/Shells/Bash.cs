using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

/// <summary>
/// Bash shell implementation with WSL support on Windows.
/// Automatically detects WSL environment and handles Windows↔WSL path mapping.
/// Supports Cygwin, MinGW, and MSYS2 environments.
/// </summary>

public class Bash : ShellBase {

    static Bash() {
        // Detect if running Bash on WSL by checking uname output
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            var shell = new CommandLine();
            var script = new ShellScript(shell, @"bash -c ""uname -s""");
            using IShellWorker worker = script.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            using var reader = new StreamReader(worker.StandardOutput);
            IsRunningBashOnWSL = reader.ReadToEnd().TrimEnd() switch {
                "Linux" => true,
                "Darwin" => true,
                _ => false
            };
        } else {
            IsRunningBashOnWSL = false;
        }
    }

    /// <summary>
    /// True if Bash is running on WSL, enabling automatic path translation.
    /// </summary>
    internal static bool IsRunningBashOnWSL { get; }

    public override ProcessWorkerBase CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) {
        return new BashWorker(shellScript, inputStream, disposeOnCompletion);
    }

    public override string MapToHostPath(string internalPath) {
        string hostPath;
        if (IsRunningBashOnWSL && Path.IsPathRooted(internalPath)) {
            hostPath = WSLUtils.WslToWindowsPath(internalPath);
        } else {
            hostPath = Path.GetFullPath(internalPath);
        }

        return hostPath;
    }

    public override string MapToInternalPath(string hostPath) {
        string internalPath;
        if (IsRunningBashOnWSL) {
            hostPath = Path.GetFullPath(hostPath);
            internalPath = WSLUtils.WindowsToWslPath(hostPath);
        } else {
            internalPath = Path.GetFullPath(hostPath);
        }

        return internalPath;
    }
}
