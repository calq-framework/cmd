namespace CalqFramework.Cmd.Shell {

    public interface IShellWorker : IDisposable {
        IShellWorker? PipedWorker { get; }
        ShellScript ShellScript { get; }
        ShellWorkerOutputStream StandardOutput { get; }

        Task EnsurePipeIsCompletedAsync(CancellationToken cancellationToken = default);

        Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default);

        Task StartAsync(CancellationToken cancellationToken = default);
    }
}
