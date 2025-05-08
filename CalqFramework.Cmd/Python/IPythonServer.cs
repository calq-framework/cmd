using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells {
    public interface IPythonServer {
        Uri Uri { get; }

        Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default);
    }
}