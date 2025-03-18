using CalqFramework.Cmd.SystemProcess;
using System.Diagnostics;

namespace CalqFramework.Cmd.Shells {
    public interface IShell {
        string GetInternalPath(string hostPath);

        void Run(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);

        Task RunAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);

        Process Start(string script, IProcessStartConfiguration processStartConfiguration, CancellationToken cancellationToken = default);
    }
}
