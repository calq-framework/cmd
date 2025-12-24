namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Base implementation for shell workers. Handles piping, lifecycle management,
    /// and provides common functionality for both process and HTTP workers.
    /// </summary>

    public abstract class ShellWorkerBase(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) : IShellWorker {
        private readonly SemaphoreSlim _hasStartedSemaphore = new SemaphoreSlim(1, 1);
        private volatile bool _hasStarted;
        private bool _disposed;

        ~ShellWorkerBase() {
            Dispose(false);
        }

        public bool DisposeOnCompletion { get; } = disposeOnCompletion;

        public Stream? InputStream { get; private set; } = inputStream;

        public IShellWorker? PipedWorker { get; private set; }

        public ShellScript ShellScript { get; } = shellScript;

        public abstract ShellWorkerOutputStream StandardOutput { get; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task EnsurePipeIsCompletedAsync(CancellationToken cancellationToken = default) {
            await EnsureStandardOutputIsReadToEndAsync(cancellationToken);
            if (PipedWorker != null) {
                await PipedWorker.EnsurePipeIsCompletedAsync(cancellationToken);
            }
        }

        public abstract Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default);

        public async Task StartAsync(CancellationToken cancellationToken = default) {
            if (_hasStarted) {
                return;
            }

            await _hasStartedSemaphore.WaitAsync(cancellationToken);
            try {
                if (_hasStarted) {
                    return;
                }

                if (ShellScript.PipedShellScript != null) {
                    PipedWorker = await ShellScript.PipedShellScript.StartAsync(cancellationToken);
                    InputStream = PipedWorker.StandardOutput;
                }

                await InitializeAsync(ShellScript, cancellationToken);
                _hasStarted = true;
            } finally {
                _hasStartedSemaphore.Release();
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _hasStartedSemaphore?.Dispose();
                }
                
                PipedWorker?.Dispose();

                _disposed = true;
            }
        }

        protected abstract Task InitializeAsync(ShellScript shellScript, CancellationToken cancellationToken = default);

        private async Task EnsureStandardOutputIsReadToEndAsync(CancellationToken cancellationToken = default) {
            await StreamUtils.RelayStream(StandardOutput, Stream.Null, cancellationToken);
        }
    }
}
