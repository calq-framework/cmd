using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class Bash : ShellBase {

    static Bash() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            var shell = new CommandLine();
            var command = new ShellCommand(shell, @"bash -c ""uname -s""") {
                In = TextReader.Null
            };
            using var worker = command.Start();
            IsRunningBashOnWSL = worker.StandardOutput.ReadToEnd().TrimEnd() switch {
                "Linux" => true,
                "Darwin" => true,
                _ => false
            };
        } else {
            IsRunningBashOnWSL = false;
        }
    }

    internal static bool IsRunningBashOnWSL { get; }

    public override string MapToInternalPath(string hostPath) {
        if (IsRunningBashOnWSL) {
            return WSLUtils.WindowsToWslPath(hostPath);
        }

        return hostPath;
    }

    public override ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, CancellationToken cancellationToken = default) {
        return new BashWorker(shellCommand, cancellationToken);
    }
}
