using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class CommandLine : ShellBase {
    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }

    public override ShellWorkerBase CreateShellWorker(string script, IShellCommandStartConfiguration shellCommandStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default) {
        return new CommandLineWorker(script, shellCommandStartConfiguration, cancellationToken) {
            PipedWorker = pipedWorker
        };
    }
}
