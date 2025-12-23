using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

public class Bash : ShellBase {

    static Bash() {
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
