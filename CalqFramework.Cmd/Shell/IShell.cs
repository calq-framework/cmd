using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        string MapToInternalPath(string hostPath);
        void Run(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);
        Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);
        Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, ShellWorkerBase? pipedShellWorker, CancellationToken cancellationToken = default);
        ShellWorkerBase Start(string script, IProcessStartConfiguration processStartConfiguration, ShellWorkerBase? pipedShellWorker, CancellationToken cancellationToken = default);
    }
}