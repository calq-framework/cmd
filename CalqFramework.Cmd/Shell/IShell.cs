using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shell {
    public interface IShell {
        string MapToInternalPath(string hostPath);

        void Run(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);

        Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);
        Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, RunnableProcess? pipedProcess, CancellationToken cancellationToken = default);

        RunnableProcess Start(string script, IProcessStartConfiguration processStartConfiguration, RunnableProcess? pipedProcess, CancellationToken cancellationToken = default);
        RunnableProcess Start(string script, IProcessRunConfiguration processRunConfiguration, RunnableProcess? pipedProcess, CancellationToken cancellationToken = default);
    }
}
