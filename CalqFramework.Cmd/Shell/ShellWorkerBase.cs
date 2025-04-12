using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public abstract class ShellWorkerBase : IDisposable {
        private bool _disposed;

        private AutoTerminateProcess _process;

        private Task? RelayInputTask;

        public ShellWorkerBase(ShellCommand shellCommand, TextReader? inputReader, CancellationToken cancellationToken = default) {
            ShellCommand = shellCommand;

            if (ShellCommand.PipedShellCommand != null) {
                PipedWorker = ShellCommand.PipedShellCommand.Shell.CreateShellWorker(ShellCommand.PipedShellCommand);
                inputReader = PipedWorker.StandardOutput;
            }

            var processExecutionInfo = GetProcessExecutionInfo(ShellCommand.WorkingDirectory, ShellCommand.Script);

            var redirectInput = inputReader != null ? true : false;
            _process = new AutoTerminateProcess() {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = ShellCommand.WorkingDirectory,
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

        public ShellCommand ShellCommand { get; }

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

            ShellCommand.Shell.ErrorHandler.AssertSuccess(ShellCommand.Script, _process.ExitCode, errorMessage, output);
        }

        internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);
    }
}
