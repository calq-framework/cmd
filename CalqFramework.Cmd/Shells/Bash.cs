using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

public class Bash : ShellBase {

    static Bash() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            var shell = new CommandLine();
            var script = new ShellScript(shell, @"bash -c ""uname -s""");
            using var worker = script.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

    public override ProcessWorkerBase CreateShellWorker(ShellScript shellScript, Stream? inputStream) {
        return new BashWorker(shellScript, inputStream);
    }

    public override string MapToHostPath(string internalPth) {
        if (IsRunningBashOnWSL) {
            return WSLUtils.WindowsToWslPath(internalPth);
        }

        return internalPth;
    }

    public override string MapToInternalPath(string hostPath) {
        if (IsRunningBashOnWSL) {
            return WSLUtils.WindowsToWslPath(hostPath);
        }

        return hostPath;
    }
}