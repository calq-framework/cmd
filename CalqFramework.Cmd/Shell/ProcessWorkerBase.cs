using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public abstract class ProcessWorkerBase : ShellWorkerBase {
        private bool _disposed;
        private AutoTerminateProcess _process = null!;
        private Task? _relayInputTask;

        // initialized via InitializeAsync

        public ProcessWorkerBase(ShellScript shellScript, Stream? inputStream) : base(shellScript, inputStream) {
        }

        public override Stream StandardOutput { get => new ProcessStream(_process); }

        protected override int CompletionCode => _process.ExitCode;
        internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                _process.Dispose();

                _disposed = true;
            }
            base.Dispose(disposing);
        }

        protected override async Task InitializeAsync(ShellScript shellScript, bool redirectInput, CancellationToken cancellationToken = default) {
            var processExecutionInfo = GetProcessExecutionInfo(ShellScript.WorkingDirectory, ShellScript.Script);

            _process = new AutoTerminateProcess() {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = ShellScript.WorkingDirectory,
                    FileName = processExecutionInfo.FileName,
                    RedirectStandardInput = redirectInput,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = processExecutionInfo.Arguments,
                },
                EnableRaisingEvents = true
            };

            var relayInputTaskAbortCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _process.Exited += (s, e) => {
                relayInputTaskAbortCts.Cancel();
            };

            _process.Start();

            if (redirectInput) {
                _relayInputTask = Task.Run(async () => await StreamUtils.RelayInput(_process.StandardInput!, new StreamReader(InputStream!), relayInputTaskAbortCts.Token)).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock thread
            }
        }

        protected override async Task<string> ReadErrorMessageAsync() {
            return await _process.StandardError.ReadToEndAsync();
        }
        protected override async Task WaitForCompletionAsync() {
            await _process.WaitForExitAsync();

            try {
                if (_relayInputTask != null) {
                    await _relayInputTask;
                }
            } catch (TaskCanceledException) { } // triggered by relayInputTaskAbortCts which should be ignored
        }
    }
}
