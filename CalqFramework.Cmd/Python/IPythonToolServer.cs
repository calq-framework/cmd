using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Python {

    public interface IPythonToolServer {
        Uri Uri { get; }

        Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default);
    }
}
