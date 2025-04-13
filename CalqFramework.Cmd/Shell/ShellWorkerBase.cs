namespace CalqFramework.Cmd.Shell {
    public abstract class ShellWorkerBase : IShellWorker {
        private bool _disposed;
        private Task? _relayInputTask;

        public ShellWorkerBase(ShellScript shellScript, TextReader? inputReader, CancellationToken cancellationToken = default) {
            ShellScript = shellScript;

            if (ShellScript.PipedShellScript != null) {
                PipedWorker = ShellScript.PipedShellScript.Shell.CreateShellWorker(ShellScript.PipedShellScript);
                inputReader = PipedWorker.StandardOutput;
            }

            RelayInputTaskAbortCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var redirectInput = inputReader != null ? true : false;
            var workerInput = Initialize(shellScript, redirectInput);

            if (redirectInput) {
                _relayInputTask = Task.Run(async () => await StreamUtils.RelayInput(workerInput, inputReader!, RelayInputTaskAbortCts.Token)).WaitAsync(RelayInputTaskAbortCts.Token); // input reading may lock thread
            }
        }

        ~ShellWorkerBase() {
            Dispose(false);
        }

        public IShellWorker? PipedWorker { get; }

        public ShellScript ShellScript { get; }

        public abstract TextReader StandardOutput { get; }

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>input reader</returns>
        protected abstract TextWriter? Initialize(ShellScript shellScript, bool redirectInput);
        protected abstract Task WaitForCompletionAsync();
    }
}
