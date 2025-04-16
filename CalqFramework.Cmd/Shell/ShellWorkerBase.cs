namespace CalqFramework.Cmd.Shell {
    public abstract class ShellWorkerBase : IShellWorker {
        private bool _disposed;
        private Task? _relayInputTask;

        public ShellWorkerBase(ShellScript shellScript, Stream? inputStream, CancellationToken cancellationToken = default) {
            ShellScript = shellScript;
            InputStream = inputStream;

            if (ShellScript.PipedShellScript != null) {
                PipedWorker = ShellScript.PipedShellScript.Shell.CreateShellWorker(ShellScript.PipedShellScript);
            }

            RelayInputTaskAbortCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public async Task Start() {
            if (PipedWorker != null) {
                await PipedWorker.Start();
                InputStream = PipedWorker.StandardOutput.BaseStream;
            }

            var redirectInput = InputStream != null ? true : false;
            var workerInput = await Initialize(ShellScript, redirectInput);

            if (workerInput != null) {
                _relayInputTask = Task.Run(async () => await StreamUtils.RelayInput(workerInput!, new StreamReader(InputStream!), RelayInputTaskAbortCts.Token)).WaitAsync(RelayInputTaskAbortCts.Token); // input reading may lock thread
            }
        }

        ~ShellWorkerBase() {
            Dispose(false);
        }

        public IShellWorker? PipedWorker { get; }

        public ShellScript ShellScript { get; }
        public Stream? InputStream { get; private set; }
        public abstract StreamReader StandardOutput { get; }

        protected abstract int CompletionCode { get; }

        protected CancellationTokenSource RelayInputTaskAbortCts { get; private set; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task WaitForSuccess(string? output = null) {
            if (PipedWorker != null) {
                await PipedWorker.WaitForSuccess();
            }
            await WaitForCompletionAsync();

            try {
                if (_relayInputTask != null) {
                    await _relayInputTask;
                }
            } catch (TaskCanceledException) { } // triggered by relayInputTaskAbortCts which should be ignored


            var errorMessage = await ReadErrorMessageAsync();

            ShellScript.Shell.ErrorHandler.AssertSuccess(ShellScript.Script, CompletionCode, errorMessage, output);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                PipedWorker?.Dispose();

                _disposed = true;
            }
        }

        protected abstract Task<string> ReadErrorMessageAsync();

        protected abstract Task<TextWriter?> Initialize(ShellScript shellScript, bool redirectInput);
        protected abstract Task WaitForCompletionAsync();
    }
}
