using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class Bash : ShellBase {

    static Bash() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            using var worker = new CommandLineWorker(@"bash -c ""uname -s""", new ShellCommandStartConfiguration() { In = TextReader.Null });
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

    public override ShellWorkerBase CreateShellWorker(string script, IShellCommandStartConfiguration shellCommandStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default) {
        return new BashWorker(script, shellCommandStartConfiguration, cancellationToken) {
            PipedWorker = pipedWorker
        };
    }
}
