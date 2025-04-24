
namespace CalqFramework.Cmd.Shell {
    public interface IShellWorker : IDisposable {
        IShellWorker? PipedWorker { get; }
        ShellScript ShellScript { get; }
        ExecutionOutputStream StandardOutput { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

        Task WaitForSuccessAsync(string? output = null, CancellationToken cancellationToken = default);
    }
}