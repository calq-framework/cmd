using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class Bash : ShellBase {

    static Bash() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            var shell = new CommandLine();
            var script = new ShellScript(shell, @"bash -c ""uname -s""");
            using var worker = script.Start().ConfigureAwait(false).GetAwaiter().GetResult();
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

    public override ProcessWorkerBase CreateShellWorker(ShellScript shellScript, TextReader? inputReader, CancellationToken cancellationToken = default) {
        return new BashWorker(shellScript, inputReader, cancellationToken);
    }
}
