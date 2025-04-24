namespace CalqFramework.Cmd.Shell {
    public abstract class ShellWorkerBase : IShellWorker {
        private bool _disposed;

        public ShellWorkerBase(ShellScript shellScript, Stream? inputStream) {
            ShellScript = shellScript;
            InputStream = inputStream;

            if (ShellScript.PipedShellScript != null) {
                PipedWorker = ShellScript.PipedShellScript.Shell.CreateShellWorker(ShellScript.PipedShellScript);
            }
        }

        ~ShellWorkerBase() {
            Dispose(false);
        }

        public Stream? InputStream { get; private set; }

        public IShellWorker? PipedWorker { get; }

        public ShellScript ShellScript { get; }

        public abstract ExecutionOutputStream StandardOutput { get; }

        protected abstract int CompletionCode { get; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default) {
            if (PipedWorker != null) {
                await PipedWorker.StartAsync();
                InputStream = PipedWorker.StandardOutput;
            }

            var redirectInput = InputStream != null ? true : false;
            await InitializeAsync(ShellScript, redirectInput, cancellationToken);
        }
        public async Task WaitForSuccessAsync(string? output = null, CancellationToken cancellationToken = default) {
            if (PipedWorker != null) {
                await PipedWorker.WaitForSuccessAsync();
            }
            await WaitForCompletionAsync();


            var errorMessage = await ReadErrorMessageAsync();

            ShellScript.Shell.ErrorHandler.AssertSuccess(ShellScript.Script, CompletionCode, errorMessage, output);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                PipedWorker?.Dispose();

                _disposed = true;
            }
        }

        protected abstract Task InitializeAsync(ShellScript shellScript, bool redirectInput, CancellationToken cancellationToken = default);

        protected abstract Task<string> ReadErrorMessageAsync();
        protected abstract Task WaitForCompletionAsync();
    }
}
