using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shells;
public class Bash : ShellBase {

    static Bash() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            using var worker = new CommandLineWorker(@"bash -c ""uname -s""", new ProcessRunConfiguration() { In = TextReader.Null });
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

    internal override ShellWorkerBase CreateShellWorker(string script, IProcessStartConfiguration processStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default) {
        return new BashWorker(script, processStartConfiguration, cancellationToken) {
            PipedWorker = pipedWorker
        };
    }
}
