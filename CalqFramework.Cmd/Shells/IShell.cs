using CalqFramework.Cmd.SystemProcess;

namespace CalqFramework.Cmd.Shells {
    public interface IShell {

        Task ExecuteAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default);

        string GetInternalPath(string hostPath);
    }
}