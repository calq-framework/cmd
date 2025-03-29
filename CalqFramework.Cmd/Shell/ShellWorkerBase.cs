using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public abstract class ShellWorkerBase : IDisposable {
        private bool _disposed;

        private AutoTerminateProcess _process;

        private Task RelayInputTask;

        public ShellWorkerBase(string script, IShellCommandStartConfiguration shellCommandStartConfiguration, CancellationToken cancellationToken = default) {
            Script = script;

            var processExecutionInfo = GetProcessExecutionInfo(shellCommandStartConfiguration.WorkingDirectory, script);

            ShellCommandStartConfiguration = shellCommandStartConfiguration;

            _process = new AutoTerminateProcess() {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = shellCommandStartConfiguration.WorkingDirectory,
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

            RelayInputTask = Task.Run(async () => await StreamUtils.RelayInput(_process.StandardInput, shellCommandStartConfiguration.In, shellCommandStartConfiguration.InWriter, cancellationToken)).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock thread
        }

        public ShellWorkerBase? PipedWorker { get; init; }

        public TextReader StandardOutput { get => _process.StandardOutput; }
        public string Script { get; }
        private IShellCommandStartConfiguration ShellCommandStartConfiguration { get; }

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

            ShellCommandStartConfiguration.ErrorHandler.AssertSuccess(Script, _process.ExitCode, errorMessage, output);
        }

        internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);
    }
}
