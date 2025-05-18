namespace CalqFramework.Cmd.Shell {

    public abstract class ShellWorkerBase : IShellWorker {
        private bool _disposed;

        public ShellWorkerBase(ShellScript shellScript, Stream? inputStream) {
            ShellScript = shellScript;
            InputStream = inputStream;
        }

        ~ShellWorkerBase() {
            Dispose(false);
        }

        public Stream? InputStream { get; private set; }

        public IShellWorker? PipedWorker { get; private set; }

        public ShellScript ShellScript { get; }

        public abstract ShellWorkerOutputStream StandardOutput { get; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task EnsurePipeIsCompletedAsync(CancellationToken cancellationToken = default) {
            await EnsureStandardOutputIsReadToEndAsync();
            if (PipedWorker != null) {
                await PipedWorker.EnsurePipeIsCompletedAsync();
            }
        }

        public abstract Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default);

        public async Task StartAsync(CancellationToken cancellationToken = default) {
            if (ShellScript.PipedShellScript != null) {
                PipedWorker = await ShellScript.PipedShellScript.StartAsync(cancellationToken);
                InputStream = PipedWorker.StandardOutput;
            }

            await InitializeAsync(ShellScript, cancellationToken);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
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