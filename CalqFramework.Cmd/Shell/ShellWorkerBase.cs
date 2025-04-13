using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public abstract class ShellWorkerBase : IDisposable {
        private bool _disposed;

        private AutoTerminateProcess _process;

        private Task? RelayInputTask;

        public ShellWorkerBase(ShellScript shellScript, TextReader? inputReader, CancellationToken cancellationToken = default) {
            ShellScript = shellScript;

            if (ShellScript.PipedShellScript != null) {
                PipedWorker = ShellScript.PipedShellScript.Shell.CreateShellWorker(ShellScript.PipedShellScript);
                inputReader = PipedWorker.StandardOutput;
            }

            var processExecutionInfo = GetProcessExecutionInfo(ShellScript.WorkingDirectory, ShellScript.Script);

            var redirectInput = inputReader != null ? true : false;
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
                RelayInputTask = Task.Run(async () => await StreamUtils.RelayInput(_process.StandardInput, inputReader!, relayInputTaskAbortCts.Token)).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock thread
            }
        }

        public ShellWorkerBase? PipedWorker { get; }

        public ShellScript ShellScript { get; }

        public TextReader StandardOutput { get => _process.StandardOutput; }

        public void Dispose() {
            if (!_disposed) {
                PipedWorker?.Dispose();
                _process.Dispose();

                _disposed = true;
            }
        }

        public async Task WaitForSuccess(string? output = null) {
            if (PipedWorker != null) {
                await PipedWorker.WaitForSuccess();
            }
            await _process.WaitForExitAsync();

            try {
                if (RelayInputTask != null) {
                    await RelayInputTask;
                }
            } catch (TaskCanceledException) { } // triggered by relayInputTaskAbortCts which should be ignored


            var errorMessage = await _process.StandardError.ReadToEndAsync();

            ShellScript.Shell.ErrorHandler.AssertSuccess(ShellScript.Script, _process.ExitCode, errorMessage, output);
        }

        internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);
    }
}
