namespace CalqFramework.Cmd.Shell;

/// <summary>
///     IShellWorker implementation that performs no execution.
///     Serves pre-existing output from a provided stream.
///     Does NOT extend ShellWorkerBase — avoids starting piped workers
///     (output already incorporates upstream pipeline results).
/// </summary>
internal class NullShellWorker : IShellWorker {
    private readonly PassthroughOutputStream _output;
    private bool _disposed;

    public NullShellWorker(ShellScript shellScript, Stream stream, bool disposeOnCompletion) {
        ShellScript = shellScript;
        DisposeOnCompletion = disposeOnCompletion;
        _output = new PassthroughOutputStream(stream, this);
    }

    public ShellWorkerOutputStream StandardOutput => _output;

    public bool DisposeOnCompletion { get; }

    public IShellWorker? PipedWorker => null;

    public ShellScript ShellScript { get; }

    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task EnsurePipeIsCompletedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);

    public void Dispose() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        _output.Dispose();
    }
}
