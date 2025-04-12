using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public abstract class ShellWorkerBase : IDisposable {
        private bool _disposed;

        private AutoTerminateProcess _process;

        private Task RelayInputTask;

        public ShellWorkerBase(ShellCommand shellCommand, TextReader inputReader, CancellationToken cancellationToken = default) {
            ShellCommand = shellCommand;

            if (ShellCommand.PipedShellCommand != null) {
                PipedWorker = ShellCommand.PipedShellCommand.Shell.CreateShellWorker(ShellCommand.PipedShellCommand);
                inputReader = PipedWorker.StandardOutput;
            }

            var processExecutionInfo = GetProcessExecutionInfo(ShellCommand.WorkingDirectory, ShellCommand.Script);

            _process = new AutoTerminateProcess() {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = ShellCommand.WorkingDirectory,
                    FileName = processExecutionInfo.FileName,
                    RedirectStandardInput = true, // TODO false when null input
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

            RelayInputTask = Task.Run(async () => await StreamUtils.RelayInput(_process.StandardInput, inputReader, relayInputTaskAbortCts.Token)).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock thread
        }

        public ShellWorkerBase(ShellCommand shellCommand, CancellationToken cancellationToken = default) : this(shellCommand, shellCommand.Shell.In, cancellationToken) {
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

        public async Task WaitForSuccess(StringWriter? outputWriter = null) {
            if (PipedWorker != null) {
                await PipedWorker.WaitForSuccess();
            }
            await _process.WaitForExitAsync();

            try {
                await RelayInputTask;
            } catch (TaskCanceledException) { } // triggered by relayInputTaskAbortCts which should be ignored


            var errorMessage = await _process.StandardError.ReadToEndAsync();
            string? output = null;
            if (string.IsNullOrEmpty(errorMessage) && outputWriter != null) {
                output = outputWriter.ToString();
            }

            ShellCommand.Shell.ErrorHandler.AssertSuccess(ShellCommand.Script, _process.ExitCode, errorMessage, output);
        }

        internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);
    }
}
