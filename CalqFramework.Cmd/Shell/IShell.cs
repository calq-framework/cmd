using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        string MapToInternalPath(string hostPath);
        void Run(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);
        Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);
        Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, ShellWorker? pipedShellWorker, CancellationToken cancellationToken = default);
        ShellWorker Start(string script, IProcessStartConfiguration processStartConfiguration, ShellWorker? pipedShellWorker, CancellationToken cancellationToken = default);
    }
}