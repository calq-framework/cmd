using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public abstract string MapToInternalPath(string hostPath);
    public abstract ShellWorkerBase CreateShellWorker(string script, IProcessStartConfiguration processStartConfiguration, ShellWorkerBase? pipedWorker, CancellationToken cancellationToken = default);
}
