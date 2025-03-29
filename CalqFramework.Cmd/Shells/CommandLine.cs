using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shells;
public class CommandLine : ShellBase {
    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }

    public override ShellWorkerBase CreateShellWorker(string script, IProcessStartConfiguration processStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default) {
        return new CommandLineWorker(script, processStartConfiguration, cancellationToken) {
            PipedWorker = pipedWorker
        };
    }
}
