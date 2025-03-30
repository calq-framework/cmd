using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class CommandLine : ShellBase {
    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }

    public override ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, CancellationToken cancellationToken = default) {
        return new CommandLineWorker(shellCommand, cancellationToken);
    }
}
